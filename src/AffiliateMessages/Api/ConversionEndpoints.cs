using System.Buffers;
using System.Diagnostics;
using AffiliateMessages.Configuration;
using AffiliateMessages.Services;
using CsvHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace AffiliateMessages.Api;

public static class ConversionEndpoints
{
    private const int MaxFileBytes = 1_000_000;

    public static void Map(WebApplication application)
    {
        application.MapPost("/api/conversions", HandleAsync);
        application.MapPost("/api/output/open-folder", OpenOutputFolderAsync);
    }

    private static async Task OpenOutputFolderAsync(
        HttpContext context,
        AppOptions options,
        ILogger<LocalUiHost> logger)
    {
        var outputPath = Path.GetFullPath(options.OutputFile);
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (outputDirectory is null || !File.Exists(outputPath) || !Directory.Exists(outputDirectory))
        {
            await ApiProblemDetails.WriteAsync(
                context,
                StatusCodes.Status404NotFound,
                "output_folder_not_available",
                "Pasta de saída indisponível",
                "O arquivo ou a pasta de saída ainda não está disponível.");
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo("explorer.exe")
            {
                UseShellExecute = true
            };
            startInfo.ArgumentList.Add(outputDirectory);
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                throw new InvalidOperationException("O Explorer não aceitou o caminho da pasta.");
            }
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Não foi possível abrir a pasta de saída configurada.");
            await ApiProblemDetails.WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "output_folder_open_failed",
                "Não foi possível abrir a pasta",
                "O Windows não conseguiu abrir a pasta de saída.");
        }
    }

    private static async Task HandleAsync(
        HttpContext context,
        AppOptions options,
        MessageBatchProcessor processor,
        ConversionState state,
        ILogger<LocalUiHost> logger)
    {
        if (!state.TryBegin())
        {
            await ApiProblemDetails.WriteAsync(
                context,
                StatusCodes.Status409Conflict,
                "conversion_in_progress",
                "Conversão em andamento",
                "Aguarde a conversão atual terminar antes de iniciar outra.");
            return;
        }

        string? temporaryPath = null;
        try
        {
            if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var contentType) ||
                !string.Equals(contentType.MediaType.Value, "multipart/form-data", StringComparison.OrdinalIgnoreCase) ||
                !contentType.Boundary.HasValue)
            {
                await ApiProblemDetails.WriteAsync(
                    context,
                    StatusCodes.Status415UnsupportedMediaType,
                    "unsupported_media_type",
                    "Formato não suportado",
                    "Envie o arquivo usando multipart/form-data.");
                return;
            }

            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary))
            {
                await ApiProblemDetails.WriteAsync(
                    context,
                    StatusCodes.Status415UnsupportedMediaType,
                    "unsupported_media_type",
                    "Formato não suportado",
                    "Envie o arquivo usando multipart/form-data.");
                return;
            }

            var reader = new MultipartReader(boundary, context.Request.Body);
            var sawFile = false;
            MultipartSection? section;

            while ((section = await reader.ReadNextSectionAsync(context.RequestAborted)) is not null)
            {
                if (!TryGetFileName(section, out var fileName) ||
                    !string.Equals(GetFieldName(section), "file", StringComparison.Ordinal) ||
                    sawFile)
                {
                    await InvalidFileAsync(context);
                    return;
                }

                if (!string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase))
                {
                    await InvalidFileAsync(context);
                    return;
                }

                sawFile = true;
                temporaryPath = Path.Combine(Path.GetTempPath(), $"AffiliateMessages-{Guid.NewGuid():N}.csv");
                var bytesCopied = await CopyWithLimitAsync(section.Body, temporaryPath, context.RequestAborted);
                if (bytesCopied > MaxFileBytes)
                {
                    await ApiProblemDetails.WriteAsync(
                        context,
                        StatusCodes.Status413PayloadTooLarge,
                        "file_too_large",
                        "Arquivo muito grande",
                        "O arquivo CSV deve ter no máximo 1.000.000 bytes.");
                    return;
                }

                if (bytesCopied == 0)
                {
                    await InvalidFileAsync(context);
                    return;
                }
            }

            if (!sawFile)
            {
                await InvalidFileAsync(context);
                return;
            }

            BatchProcessResult result;
            try
            {
                result = await processor.ProcessAsync(temporaryPath!, options.OutputFile, context.RequestAborted);
            }
            catch (CsvHelperException exception)
            {
                logger.LogWarning(exception, "O arquivo enviado não possui uma estrutura CSV válida.");
                await ApiProblemDetails.WriteAsync(
                    context,
                    StatusCodes.Status422UnprocessableEntity,
                    "invalid_csv",
                    "CSV inválido",
                    "Não foi possível ler a estrutura do arquivo CSV.");
                return;
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha ao processar o lote CSV.");
                await ApiProblemDetails.WriteAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "conversion_failed",
                    "Falha na conversão",
                    "Não foi possível concluir a conversão do arquivo.");
                return;
            }

            var outputPath = Path.GetFullPath(options.OutputFile);
            var outputDirectory = Path.GetDirectoryName(outputPath);
            var response = new ConversionResponse(
                result.Messages.Select((text, index) => new GeneratedMessageResponse(index + 1, text)).ToArray(),
                outputPath,
                File.Exists(outputPath) && outputDirectory is not null && Directory.Exists(outputDirectory),
                result.InvalidRowsCount,
                result.MessagesWithoutPhrase);
            await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            return;
        }
        catch (InvalidDataException exception)
        {
            logger.LogWarning(exception, "A requisição multipart contém dados inválidos.");
            await InvalidFileAsync(context);
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Falha ao receber o arquivo CSV em armazenamento temporário.");
            await ApiProblemDetails.WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "conversion_failed",
                "Falha na conversão",
                "Não foi possível receber ou processar o arquivo.");
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    logger.LogWarning(exception, "Não foi possível remover o arquivo temporário de upload.");
                }
            }

            state.End();
        }
    }

    private static string GetFieldName(MultipartSection section)
    {
        if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition) ||
            !disposition.Name.HasValue)
        {
            return string.Empty;
        }

        return HeaderUtilities.RemoveQuotes(disposition.Name).Value ?? string.Empty;
    }

    private static bool TryGetFileName(MultipartSection section, out string fileName)
    {
        fileName = string.Empty;
        if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition) ||
            !string.Equals(disposition.DispositionType.Value, "form-data", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var value = disposition.FileNameStar.HasValue
            ? disposition.FileNameStar
            : disposition.FileName;
        if (!value.HasValue)
        {
            return false;
        }

        fileName = HeaderUtilities.RemoveQuotes(value).Value ?? string.Empty;
        return !string.IsNullOrWhiteSpace(fileName);
    }

    private static async Task<long> CopyWithLimitAsync(
        Stream source,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            long totalBytes = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                totalBytes += bytesRead;
                if (totalBytes > MaxFileBytes)
                {
                    return totalBytes;
                }

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            return totalBytes;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static Task InvalidFileAsync(HttpContext context)
    {
        return ApiProblemDetails.WriteAsync(
            context,
            StatusCodes.Status400BadRequest,
            "invalid_file",
            "Arquivo inválido",
            "Selecione exatamente um arquivo CSV não vazio.");
    }

    private sealed record ConversionResponse(
        IReadOnlyList<GeneratedMessageResponse> Messages,
        string OutputPath,
        bool CanOpenOutputFolder,
        int InvalidRowsCount,
        int MessagesWithoutPhrase);

    private sealed record GeneratedMessageResponse(int Sequence, string Text);
}
