using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using AffiliateMessages.Configuration;
using AffiliateMessages.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace AffiliateMessages.Tests.Performance;

public sealed class BatchProcessingPerformanceTests
{
    private readonly ITestOutputHelper output;

    public BatchProcessingPerformanceTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    [Trait("Category", "Performance")]
    public async Task FiftyValidProducts_CompleteWithLocalOllamaWithinThreeMinutes()
    {
        var options = new AppOptions();
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var versionResponse = await httpClient.GetAsync($"{options.OllamaUrl.TrimEnd('/')}/api/version");
        Assert.True(versionResponse.IsSuccessStatusCode,
            $"Local Ollama is required at {options.OllamaUrl} to run this opt-in benchmark.");
        var ollamaVersion = await versionResponse.Content.ReadFromJsonAsync<OllamaVersion>();
        Assert.False(string.IsNullOrWhiteSpace(ollamaVersion?.Version),
            "The local Ollama version could not be determined.");
        var installedModels = await httpClient.GetFromJsonAsync<OllamaModels>(
            $"{options.OllamaUrl.TrimEnd('/')}/api/tags");
        Assert.Contains(installedModels!.Models,
            model => string.Equals(model.Name, options.OllamaModel, StringComparison.OrdinalIgnoreCase));

        var fixture = LoadFixture();
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var inputPath = Path.Combine(directory, "products.csv");
        var outputPath = Path.Combine(directory, "messages.txt");
        var logPath = Path.Combine(directory, "messages.log");
        await File.WriteAllBytesAsync(inputPath, fixture);

        using var ollamaHttpClient = new HttpClient();
        var processor = new MessageBatchProcessor(
            new CsvReader(),
            new OllamaClient(
                ollamaHttpClient,
                options,
                NullLogger<OllamaClient>.Instance),
            new InvalidLineLogger(logPath),
            new MessageWriter(),
            options.Divider);

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await processor.ProcessAsync(inputPath, outputPath);
            stopwatch.Stop();

            Assert.Equal(50, result.Messages.Count);
            Assert.True(File.Exists(outputPath));
            Assert.True(stopwatch.Elapsed < TimeSpan.FromMinutes(3),
                $"Conversion took {stopwatch.Elapsed}; the limit is three minutes.");
            output.WriteLine(
                "Reference environment: {0}; architecture: {1}; CPUs: {2}; available memory: {3} bytes; .NET: {4}; Ollama: {5}; model: {6}; elapsed: {7}.",
                Environment.OSVersion.VersionString,
                RuntimeInformation.ProcessArchitecture,
                Environment.ProcessorCount,
                GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
                RuntimeInformation.FrameworkDescription,
                ollamaVersion!.Version,
                options.OllamaModel,
                stopwatch.Elapsed);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static byte[] LoadFixture()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetManifestResourceNames()
            .Single(resourceName => resourceName.EndsWith("representative-50-products.csv", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException("The embedded performance CSV fixture was not found.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private sealed record OllamaVersion(string? Version);
    private sealed record OllamaModels(IReadOnlyList<OllamaModel> Models);
    private sealed record OllamaModel(string Name);
}
