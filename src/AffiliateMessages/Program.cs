using AffiliateMessages.Configuration;
using AffiliateMessages.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var argsByName = ParseArguments(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddCommandLine(args)
    .Build();

var options = new AppOptions();
options.OllamaUrl = configuration["OllamaUrl"] ?? options.OllamaUrl;
options.OllamaModel = configuration["OllamaModel"] ?? options.OllamaModel;
options.InputCsv = configuration["InputCsv"] ?? options.InputCsv;
options.OutputFile = configuration["OutputFile"] ?? options.OutputFile;
options.LogFile = configuration["LogFile"] ?? options.LogFile;
options.Divider = configuration["Divider"] ?? options.Divider;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddConsole();
});
var logger = loggerFactory.CreateLogger("AffiliateMessages");

var inputFile = GetArgument(argsByName, "input") ?? options.InputCsv;
var outputFile = GetArgument(argsByName, "output") ?? options.OutputFile;
var logFile = GetArgument(argsByName, "log") ?? options.LogFile;
var ollamaUrl = GetArgument(argsByName, "ollama-url") ?? options.OllamaUrl;
var model = GetArgument(argsByName, "model") ?? options.OllamaModel;

options.InputCsv = inputFile;
options.OutputFile = outputFile;
options.LogFile = logFile;
options.OllamaUrl = ollamaUrl;
options.OllamaModel = model;

if (!File.Exists(inputFile))
{
    logger.LogError("Arquivo CSV nao encontrado: {InputFile}", inputFile);
    return 1;
}

File.WriteAllText(logFile, string.Empty);

using var httpClient = new HttpClient();
var processor = new MessageBatchProcessor(
    new CsvReader(),
    new OllamaClient(httpClient, options, loggerFactory.CreateLogger<OllamaClient>()),
    new InvalidLineLogger(logFile),
    new MessageWriter(),
    options.Divider);

await processor.ProcessAsync(inputFile, outputFile);
logger.LogInformation("Mensagens gravadas em {OutputFile}", outputFile);
return 0;

static IReadOnlyDictionary<string, string> ParseArguments(string[] arguments)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 0; index < arguments.Length - 1; index++)
    {
        if (arguments[index].StartsWith("--", StringComparison.Ordinal))
        {
            var name = arguments[index][2..];
            result[name] = arguments[index + 1];
        }
    }
    return result;
}

static string? GetArgument(IReadOnlyDictionary<string, string> arguments, string name)
{
    return arguments.TryGetValue(name, out var value) ? value : null;
}
