using System.Net;
using System.Text;
using AffiliateMessages.Configuration;
using AffiliateMessages.Services;

namespace AffiliateMessages.Tests.Services;

public sealed class MessageBatchProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsMessagesAndWritesTheSameBatch()
    {
        using var files = new TemporaryFiles();
        File.WriteAllText(files.Input,
            "1,Produto valido,\"79,19\",10,Loja,5,3,https://produto.example,https://afiliado.example");
        var processor = CreateProcessor(new PhraseHandler("OFERTA TOP"), files.Log);

        var result = await processor.ProcessAsync(files.Input, files.Output);

        Assert.Equal(0, result.InvalidRowsCount);
        Assert.Equal(0, result.MessagesWithoutPhrase);
        Assert.Single(result.Messages);
        Assert.Contains("Produto valido", result.Messages[0]);
        Assert.Equal(result.Messages[0], File.ReadAllText(files.Output));
    }

    [Fact]
    public async Task ProcessAsync_ContinuesAfterInvalidRowsAndCountsThem()
    {
        using var files = new TemporaryFiles();
        File.WriteAllText(files.Input, string.Join(Environment.NewLine,
        [
            "1,Produto valido,\"79,19\",10,Loja,5,3,https://produto.example,https://afiliado.example",
            "2,Produto invalido,\"79,19\",10,Loja,5,3,https://produto.example,"
        ]));
        var processor = CreateProcessor(new PhraseHandler("OFERTA"), files.Log);

        var result = await processor.ProcessAsync(files.Input, files.Output);

        Assert.Equal(1, result.InvalidRowsCount);
        Assert.Single(result.Messages);
        Assert.Contains("Linha 2:", File.ReadAllText(files.Log));
    }

    [Fact]
    public async Task ProcessAsync_ContinuesWhenOllamaFailsAndCountsMessagesWithoutPhrase()
    {
        using var files = new TemporaryFiles();
        File.WriteAllText(files.Input, string.Join(Environment.NewLine,
        [
            "1,Produto 1,\"79,19\",10,Loja,5,3,https://produto.example/1,https://afiliado.example/1",
            "2,Produto 2,\"89,19\",10,Loja,5,3,https://produto.example/2,https://afiliado.example/2"
        ]));
        var processor = CreateProcessor(new PhraseHandler(null, HttpStatusCode.ServiceUnavailable), files.Log);

        var result = await processor.ProcessAsync(files.Input, files.Output);

        Assert.Equal(2, result.Messages.Count);
        Assert.Equal(2, result.MessagesWithoutPhrase);
        Assert.Contains("Produto 1", File.ReadAllText(files.Output));
        Assert.Contains("Produto 2", File.ReadAllText(files.Output));
    }

    private static MessageBatchProcessor CreateProcessor(HttpMessageHandler handler, string logPath)
    {
        var options = new AppOptions();
        return new MessageBatchProcessor(
            new CsvReader(),
            new OllamaClient(new HttpClient(handler), options),
            new InvalidLineLogger(logPath),
            new MessageWriter(),
            "====================");
    }

    private sealed class PhraseHandler(string? phrase, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var payload = phrase is null ? "{}" : $"{{\"response\":\"{phrase}\"}}";
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class TemporaryFiles : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public TemporaryFiles()
        {
            Directory.CreateDirectory(directory);
            Input = Path.Combine(directory, "input.csv");
            Output = Path.Combine(directory, "output.txt");
            Log = Path.Combine(directory, "log.txt");
        }

        public string Input { get; }
        public string Output { get; }
        public string Log { get; }

        public void Dispose()
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
