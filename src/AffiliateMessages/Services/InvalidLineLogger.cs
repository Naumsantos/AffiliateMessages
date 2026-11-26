namespace AffiliateMessages.Services;

public sealed class InvalidLineLogger
{
    private readonly string logFile;

    public InvalidLineLogger(string logFile)
    {
        this.logFile = logFile;
    }

    public void Log(int lineNumber, string reason)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(logFile)) ?? ".");
        File.AppendAllText(logFile, $"Linha {lineNumber}: {reason}{Environment.NewLine}");
    }
}
