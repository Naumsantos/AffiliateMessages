using System.Net;
using System.Net.Http.Json;
using AffiliateMessages.Api;
using AffiliateMessages.Configuration;

namespace AffiliateMessages.Tests.Api;

public sealed class ConversionEndpointsTests
{
    [Fact]
    public async Task MissingFile_ReturnsInvalidFileProblem()
    {
        await using var api = await LocalApiClient.StartAsync();
        using var content = new MultipartFormDataContent();

        using var response = await api.PostConversionAsync(content);

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "invalid_file");
    }

    [Fact]
    public async Task EmptyFile_ReturnsInvalidFileProblem()
    {
        await using var api = await LocalApiClient.StartAsync();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([]), "file", "empty.csv");

        using var response = await api.PostConversionAsync(content);

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "invalid_file");
    }

    [Fact]
    public async Task WrongExtension_ReturnsInvalidFileProblem()
    {
        await using var api = await LocalApiClient.StartAsync();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([1]), "file", "input.txt");

        using var response = await api.PostConversionAsync(content);

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "invalid_file");
    }

    [Fact]
    public async Task FileOverOneMillionBytes_ReturnsFileTooLargeProblem()
    {
        await using var api = await LocalApiClient.StartAsync();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[1_000_001]), "file", "input.csv");

        using var response = await api.PostConversionAsync(content);

        await AssertProblemAsync(response, HttpStatusCode.RequestEntityTooLarge, "file_too_large");
    }

    [Fact]
    public async Task ValidFile_ReturnsMessagesAndTheSavedOutputPath()
    {
        await using var api = await LocalApiClient.StartAsync();
        using var content = new MultipartFormDataContent();
        var csv = "1,Produto valido,\"79,19\",10,Loja,5,3,https://produto.example,https://afiliado.example";
        content.Add(new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(csv)), "file", "products.csv");

        using var response = await api.PostConversionAsync(content);
        var result = await response.Content.ReadFromJsonAsync<ConversionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result.Messages);
        Assert.Equal(1, result.Messages[0].Sequence);
        Assert.Contains("Produto valido", result.Messages[0].Text);
        Assert.True(File.Exists(result.OutputPath));
        Assert.Equal(result.Messages[0].Text, File.ReadAllText(result.OutputPath));
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode statusCode, string code)
    {
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains($"\"code\":\"{code}\"", body);
    }

    private sealed class LocalApiClient : IAsyncDisposable
    {
        private readonly LocalUiHost host;
        private readonly HttpClient client;
        private readonly string csrfToken;
        private readonly string directory;

        private LocalApiClient(LocalUiHost host, HttpClient client, string csrfToken, string directory)
        {
            this.host = host;
            this.client = client;
            this.csrfToken = csrfToken;
            this.directory = directory;
        }

        public static async Task<LocalApiClient> StartAsync()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var host = await LocalUiHost.StartAsync(new AppOptions
            {
                OutputFile = Path.Combine(directory, "messages.txt"),
                LogFile = Path.Combine(directory, "messages.log")
            });
            var client = new HttpClient { BaseAddress = host.Address };
            var session = await client.GetFromJsonAsync<SessionResponse>("/api/session");
            return new LocalApiClient(host, client, session!.CsrfToken, directory);
        }

        public async Task<HttpResponseMessage> PostConversionAsync(HttpContent content)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/conversions") { Content = content };
            request.Headers.TryAddWithoutValidation("Origin", host.Address.GetLeftPart(UriPartial.Authority));
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrfToken);
            return await client.SendAsync(request);
        }

        public async ValueTask DisposeAsync()
        {
            client.Dispose();
            await host.DisposeAsync();
            Directory.Delete(directory, recursive: true);
        }

        private sealed record SessionResponse(string CsrfToken);
    }

    private sealed record ConversionResponse(
        GeneratedMessageResponse[] Messages,
        string OutputPath,
        bool CanOpenOutputFolder,
        int InvalidRowsCount,
        int MessagesWithoutPhrase);

    private sealed record GeneratedMessageResponse(int Sequence, string Text);
}
