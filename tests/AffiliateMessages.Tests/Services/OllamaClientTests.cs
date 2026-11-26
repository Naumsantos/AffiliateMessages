using System.Net;
using System.Text;
using AffiliateMessages.Configuration;
using AffiliateMessages.Services;

namespace AffiliateMessages.Tests.Services;

public class OllamaClientTests
{
    [Fact]
    public async Task GeneratePhraseAsync_Success_ReturnsPhrase()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """{"model":"llama3.2:latest","response":"SEUS PES VÃO BRILHAR 😘","done":true}""");
        var client = new OllamaClient(new HttpClient(handler),
            new AppOptions { OllamaUrl = "http://localhost:11434", OllamaModel = "llama3.2:latest" });

        var result = await client.GeneratePhraseAsync("Sandalia");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GeneratePhraseAsync_Offline_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "{}");
        var client = new OllamaClient(new HttpClient(handler),
            new AppOptions { OllamaUrl = "http://localhost:11434", OllamaModel = "llama3.2:latest" });

        Assert.Null(await client.GeneratePhraseAsync("Sandalia"));
    }

    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
        }
    }
}
