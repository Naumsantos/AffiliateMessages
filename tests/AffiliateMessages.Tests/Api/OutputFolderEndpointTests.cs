using System.Net;
using System.Net.Http.Json;
using AffiliateMessages.Api;
using AffiliateMessages.Configuration;

namespace AffiliateMessages.Tests.Api;

public sealed class OutputFolderEndpointTests
{
    [Fact]
    public async Task ClientSuppliedPathIsIgnoredWhenConfiguredOutputIsUnavailable()
    {
        await using var api = await LocalApiClient.StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/output/open-folder")
        {
            Content = JsonContent.Create(new { outputPath = Path.GetTempPath() })
        };
        api.AddSecurityHeaders(request);

        using var response = await api.Client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("\"code\":\"output_folder_not_available\"", body);
    }

    private sealed class LocalApiClient : IAsyncDisposable
    {
        private readonly LocalUiHost host;
        private readonly string csrfToken;

        private LocalApiClient(LocalUiHost host, HttpClient client, string csrfToken)
        {
            this.host = host;
            Client = client;
            this.csrfToken = csrfToken;
        }

        public HttpClient Client { get; }

        public static async Task<LocalApiClient> StartAsync()
        {
            var host = await LocalUiHost.StartAsync(new AppOptions
            {
                OutputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt"),
                LogFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".log")
            });
            var client = new HttpClient { BaseAddress = host.Address };
            var session = await client.GetFromJsonAsync<SessionResponse>("/api/session");
            return new LocalApiClient(host, client, session!.CsrfToken);
        }

        public void AddSecurityHeaders(HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation("Origin", host.Address.GetLeftPart(UriPartial.Authority));
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrfToken);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await host.DisposeAsync();
        }

        private sealed record SessionResponse(string CsrfToken);
    }
}
