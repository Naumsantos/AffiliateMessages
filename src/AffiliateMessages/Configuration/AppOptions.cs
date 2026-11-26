namespace AffiliateMessages.Configuration;

public sealed class AppOptions
{
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "llama3.2:latest";
    public string InputCsv { get; set; } = "produtos.csv";
    public string OutputFile { get; set; } = "messages.txt";
    public string LogFile { get; set; } = "logs.txt";
    public string Divider { get; set; } = "====================";
}
