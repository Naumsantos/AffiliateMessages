using System.Net;
using System.Text;
using AffiliateMessages.Configuration;
using AffiliateMessages.Services;

namespace AffiliateMessages.Tests.Services;

public class ResilienceTests
{
    [Fact]
    public void InvalidLineLogger_WritesLineNumberAndReason()
    {
        var logPath = Path.GetTempFileName();
        var logger = new InvalidLineLogger(logPath);

        logger.Log(3, "Preco invalido");

        Assert.Contains("Linha 3: Preco invalido", File.ReadAllText(logPath));
    }

    [Fact]
    public async Task BatchProcessor_OllamaOffline_StillWritesValidMessages()
    {
        var csvPath = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        var logPath = Path.GetTempFileName();
        File.WriteAllText(csvPath,
            "1,Produto valido,\"79,19\",10,Loja,5,3,https://produto.example,https://afiliado.example\n");

        var handler = new OfflineHandler();
        var httpClient = new HttpClient(handler);
        var options = new AppOptions
        {
            OllamaUrl = "http://localhost:11434",
            OllamaModel = "llama3.2:latest"
        };
        var processor = new MessageBatchProcessor(
            new CsvReader(),
            new OllamaClient(httpClient, options),
            new InvalidLineLogger(logPath),
            new MessageWriter(),
            "====================");

        await processor.ProcessAsync(csvPath, outputPath);

        var output = File.ReadAllText(outputPath);
        Assert.Contains("Produto valido", output);
        Assert.Contains("✅ POR : R$ 79,19", output);
        Assert.Contains("https://afiliado.example", output);
    }

    [Fact]
    public void MessageWriter_SeparatesMessagesWithDivider()
    {
        var outputPath = Path.GetTempFileName();
        var writer = new MessageWriter();

        writer.Write(outputPath, ["Produto 1", "Produto 2"], "====================");

        Assert.Equal($"Produto 1{Environment.NewLine}===================={Environment.NewLine}Produto 2",
            File.ReadAllText(outputPath));
    }

    [Fact]
    public async Task BatchProcessor_FiftyValidProducts_CompletesWithinThreeMinutes()
    {
        var csvPath = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        var logPath = Path.GetTempFileName();
        var rows = Enumerable.Range(1, 50)
            .Select(index => $"{index},Produto {index},\"79,19\",10,Loja,5,3,https://produto.example/{index},https://afiliado.example/{index}");
        File.WriteAllText(csvPath, string.Join(Environment.NewLine, rows));

        var processor = new MessageBatchProcessor(
            new CsvReader(),
            new OllamaClient(new HttpClient(new OfflineHandler()),
                new AppOptions { OllamaUrl = "http://localhost:11434", OllamaModel = "llama3.2:latest" }),
            new InvalidLineLogger(logPath),
            new MessageWriter(),
            "====================");
        var startedAt = DateTime.UtcNow;

        await processor.ProcessAsync(csvPath, outputPath);

        Assert.True(DateTime.UtcNow - startedAt < TimeSpan.FromMinutes(3));
        Assert.Equal(49, File.ReadAllText(outputPath).Split("====================").Length - 1);
    }

    private sealed class OfflineHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        }
    }
}
