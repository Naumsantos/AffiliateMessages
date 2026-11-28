using System.Net;
using System.Net.Http.Json;
using AffiliateMessages.Api;
using AffiliateMessages.Configuration;

namespace AffiliateMessages.Tests.Api;

public sealed class LocalRequestSecurityTests
{
    [Fact]
    public async Task Session_ReturnsNoStoreCsrfToken()
    {
        await using var host = await StartHostAsync();
        using var client = new HttpClient { BaseAddress = host.Address };

        using var response = await client.GetAsync("/api/session");
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);
        Assert.False(string.IsNullOrWhiteSpace(session.CsrfToken));
        Assert.True(response.Headers.CacheControl!.NoStore);
    }

    [Fact]
    public async Task Request_WithUnexpectedHost_IsRejected()
    {
        await using var host = await StartHostAsync();
        using var client = new HttpClient { BaseAddress = host.Address };
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/session");
        request.Headers.Host = "localhost:" + host.Address.Port;

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(null, "same-origin", "valid-token")]
    [InlineData("http://evil.example", "same-origin", "valid-token")]
    [InlineData("http://127.0.0.1", "cross-site", "valid-token")]
    [InlineData("http://127.0.0.1", "same-origin", "invalid-token")]
    public async Task MutatingRequest_WithInvalidOriginSiteOrToken_IsRejected(
        string? origin,
        string fetchSite,
        string token)
    {
        await using var host = await StartHostAsync();
        using var client = new HttpClient { BaseAddress = host.Address };
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/conversions")
        {
            Content = new MultipartFormDataContent()
        };
        if (origin is not null)
        {
            request.Headers.TryAddWithoutValidation("Origin", origin);
        }
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", fetchSite);
        request.Headers.TryAddWithoutValidation("X-CSRF-Token", token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<LocalUiHost> StartHostAsync()
    {
        return await LocalUiHost.StartAsync(new AppOptions
        {
            OutputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt"),
            LogFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".log")
        });
    }

    private sealed record SessionResponse(string CsrfToken);
}
