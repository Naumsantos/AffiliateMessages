using System.Net;
using System.Security.Cryptography;
using AffiliateMessages.Configuration;
using AffiliateMessages.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AffiliateMessages.Api;

public sealed class LocalUiHost : IAsyncDisposable
{
    private readonly WebApplication application;

    private LocalUiHost(WebApplication application, Uri address, IReadOnlyList<Uri> listeningAddresses)
    {
        this.application = application;
        Address = address;
        ListeningAddresses = listeningAddresses;
    }

    public Uri Address { get; }
    public IReadOnlyList<Uri> ListeningAddresses { get; }

    public static async Task<LocalUiHost> StartAsync(AppOptions options)
    {
        var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = webRoot
        });

        builder.WebHost.ConfigureKestrel(server => server.Listen(IPAddress.Loopback, 0));
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(new LocalRequestSecurityOptions
        {
            CsrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        });
        builder.Services.AddSingleton<ConversionState>();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<MessageBatchProcessor>(services =>
        {
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            return new MessageBatchProcessor(
                new CsvReader(),
                new OllamaClient(services.GetRequiredService<HttpClient>(), options, loggerFactory.CreateLogger<OllamaClient>()),
                new InvalidLineLogger(options.LogFile),
                new MessageWriter(),
                options.Divider);
        });

        var application = builder.Build();
        application.UseExceptionHandler(errorApplication =>
        {
            errorApplication.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception is not null)
                {
                    application.Logger.LogError(exception, "Falha não tratada ao processar uma solicitação local.");
                }

                if (!context.Response.HasStarted)
                {
                    await ApiProblemDetails.WriteAsync(
                        context,
                        StatusCodes.Status500InternalServerError,
                        "internal_error",
                        "Falha interna",
                        "Não foi possível concluir a solicitação.",
                        context.RequestAborted);
                }
            });
        });

        application.UseMiddleware<LocalRequestSecurity>();
        application.UseDefaultFiles();
        application.UseStaticFiles();
        application.MapGet("/api/session", (LocalRequestSecurityOptions security) =>
            Results.Ok(new { csrfToken = security.CsrfToken }));
        ConversionEndpoints.Map(application);
        HostLifecycleEndpoints.Map(application);
        application.MapFallbackToFile("index.html");

        try
        {
            await application.StartAsync();
        }
        catch
        {
            await application.DisposeAsync();
            throw;
        }

        var addresses = application.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()?.Addresses
            .Select(address => new Uri(address))
            .ToArray() ?? [];
        if (addresses.Length != 1)
        {
            await application.DisposeAsync();
            throw new InvalidOperationException("O host local não iniciou em um único endereço.");
        }

        var address = addresses[0];
        application.Services.GetRequiredService<LocalRequestSecurityOptions>().ExpectedHost = address.Authority;

        return new LocalUiHost(application, address, addresses);
    }

    public Task WaitForShutdownAsync(CancellationToken cancellationToken = default)
    {
        return application.WaitForShutdownAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return application.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await application.DisposeAsync();
    }
}
