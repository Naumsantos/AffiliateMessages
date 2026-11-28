using System.Net;
using System.Net.Http.Json;
using AffiliateMessages.Api;
using AffiliateMessages.Configuration;

namespace AffiliateMessages.Tests.Api;

public sealed class ApiProblemDetailsTests
{
    [Fact]
    public async Task MissingUpload_ReturnsSafeProblemDetailsWithStableCode()
    {
        await using var host = await LocalUiHost.StartAsync(new AppOptions
        {
            OutputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt"),
            LogFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".log")
        });
        using var client = new HttpClient { BaseAddress = host.Address };
        var session = await client.GetFromJsonAsync<SessionResponse>("/api/session");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/conversions")
        {
            Content = new MultipartFormDataContent()
        };
        request.Headers.TryAddWithoutValidation("Origin", host.Address.GetLeftPart(UriPartial.Authority));
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
        request.Headers.TryAddWithoutValidation("X-CSRF-Token", session!.CsrfToken);

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("\"code\":\"invalid_file\"", body);
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetTempPath(), body, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record SessionResponse(string CsrfToken);
}
