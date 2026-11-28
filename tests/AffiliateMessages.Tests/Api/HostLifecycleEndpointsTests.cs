using System.Net;
using System.Net.Http.Json;
using AffiliateMessages.Api;
using AffiliateMessages.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AffiliateMessages.Tests.Api;

public sealed class HostLifecycleEndpointsTests
{
    [Fact]
    public async Task Shutdown_RequiresCsrfToken()
    {
        await using var api = await LocalApiClient.StartAsync();
        using var request = api.CreatePost("/api/host/shutdown", "invalid-token");

        using var response = await api.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Shutdown_ReturnsConflictWhileConversionIsActive()
    {
        await using var ollama = await DelayedOllama.StartAsync();
        await using var api = await LocalApiClient.StartAsync(ollama.Address.ToString());
        using var content = new MultipartFormDataContent();
        var csv = "1,Produto valido,\"79,19\",10,Loja,5,3,https://produto.example,https://afiliado.example";
        content.Add(new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(csv)), "file", "products.csv");

        var conversionTask = api.PostAsync("/api/conversions", content);
        try
        {
            await ollama.RequestStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            using var shutdown = await api.PostAsync("/api/host/shutdown");
            Assert.Equal(HttpStatusCode.Conflict, shutdown.StatusCode);
        }
        finally
        {
            ollama.ReleaseResponse();
        }

        using var conversion = await conversionTask;
        Assert.Equal(HttpStatusCode.OK, conversion.StatusCode);
    }

    [Fact]
    public async Task Shutdown_ReturnsAcceptedBeforeTheHostStops()
    {
        await using var api = await LocalApiClient.StartAsync();
        var shutdownTask = api.Host.WaitForShutdownAsync();

        using var response = await api.PostAsync("/api/host/shutdown");

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        await shutdownTask.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private sealed class LocalApiClient : IAsyncDisposable
    {
        private readonly string csrfToken;
        private readonly string directory;

        private LocalApiClient(LocalUiHost host, HttpClient client, string csrfToken, string directory)
        {
            Host = host;
            Client = client;
            this.csrfToken = csrfToken;
            this.directory = directory;
        }

        public LocalUiHost Host { get; }
        public HttpClient Client { get; }

        public static async Task<LocalApiClient> StartAsync(string? ollamaUrl = null)
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var host = await LocalUiHost.StartAsync(new AppOptions
            {
                OutputFile = Path.Combine(directory, "messages.txt"),
                LogFile = Path.Combine(directory, "messages.log"),
                OllamaUrl = ollamaUrl ?? "http://127.0.0.1:1"
            });
            var client = new HttpClient { BaseAddress = host.Address };
            var session = await client.GetFromJsonAsync<SessionResponse>("/api/session");
            return new LocalApiClient(host, client, session!.CsrfToken, directory);
        }

        public HttpRequestMessage CreatePost(string path, string? token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new ByteArrayContent([])
            };
            request.Headers.TryAddWithoutValidation("Origin", Host.Address.GetLeftPart(UriPartial.Authority));
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            request.Headers.TryAddWithoutValidation("X-CSRF-Token", token ?? csrfToken);
            return request;
        }

        public async Task<HttpResponseMessage> PostAsync(string path, HttpContent? content = null)
        {
            using var request = CreatePost(path);
            if (content is not null)
            {
                request.Content?.Dispose();
                request.Content = content;
            }

            return await Client.SendAsync(request);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Host.DisposeAsync();
            Directory.Delete(directory, recursive: true);
        }

        private sealed record SessionResponse(string CsrfToken);
    }

    private sealed class DelayedOllama : IAsyncDisposable
    {
        private readonly WebApplication application;
        private readonly TaskCompletionSource<bool> responseReleased;

        private DelayedOllama(
            WebApplication application,
            Uri address,
            TaskCompletionSource<bool> requestStarted,
            TaskCompletionSource<bool> responseReleased)
        {
            this.application = application;
            Address = address;
            RequestStarted = requestStarted;
            this.responseReleased = responseReleased;
        }

        public Uri Address { get; }
        public TaskCompletionSource<bool> RequestStarted { get; }

        public static async Task<DelayedOllama> StartAsync()
        {
            var requestStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var responseReleased = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(server => server.Listen(System.Net.IPAddress.Loopback, 0));
            var application = builder.Build();
            application.MapPost("/api/generate", async (HttpContext context) =>
            {
                requestStarted.TrySetResult(true);
                await responseReleased.Task.WaitAsync(context.RequestAborted);
                await context.Response.WriteAsJsonAsync(new { response = "OFERTA ESPECIAL" });
            });
            await application.StartAsync();
            var address = application.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses
                .Select(value => new Uri(value))
                .Single();
            return new DelayedOllama(application, address, requestStarted, responseReleased);
        }

        public void ReleaseResponse()
        {
            responseReleased.TrySetResult(true);
        }

        public async ValueTask DisposeAsync()
        {
            ReleaseResponse();
            await application.DisposeAsync();
        }

    }
}
