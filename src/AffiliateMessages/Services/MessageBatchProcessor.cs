using AffiliateMessages.Models;

namespace AffiliateMessages.Services;

public sealed class MessageBatchProcessor
{
    private readonly CsvReader csvReader;
    private readonly OllamaClient ollamaClient;
    private readonly InvalidLineLogger invalidLineLogger;
    private readonly MessageWriter messageWriter;
    private readonly string divider;

    public MessageBatchProcessor(
        CsvReader csvReader,
        OllamaClient ollamaClient,
        InvalidLineLogger invalidLineLogger,
        MessageWriter messageWriter,
        string divider)
    {
        this.csvReader = csvReader;
        this.ollamaClient = ollamaClient;
        this.invalidLineLogger = invalidLineLogger;
        this.messageWriter = messageWriter;
        this.divider = divider;
    }

    public async Task<BatchProcessResult> ProcessAsync(
        string inputFile,
        string outputFile,
        CancellationToken cancellationToken = default)
    {
        var entries = csvReader.Read(inputFile);
        var messages = new List<string>();
        var invalidRowsCount = 0;
        var messagesWithoutPhrase = 0;

        foreach (var entry in entries)
        {
            if (entry.InvalidReason is not null)
            {
                invalidLineLogger.Log(entry.LineNumber, entry.InvalidReason);
                invalidRowsCount++;
                continue;
            }

            var product = entry.Product!;
            var phrase = await ollamaClient.GeneratePhraseAsync(product.Name ?? string.Empty, cancellationToken);
            if (phrase is null)
            {
                messagesWithoutPhrase++;
            }
            messages.Add(MessageFormatter.Format(product, phrase?.ToUpper()));
        }

        messageWriter.Write(outputFile, messages, divider);
        return new BatchProcessResult(messages, invalidRowsCount, messagesWithoutPhrase);
    }
}
