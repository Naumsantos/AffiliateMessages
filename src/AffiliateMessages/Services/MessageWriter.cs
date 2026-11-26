using System.Text;

namespace AffiliateMessages.Services;

public sealed class MessageWriter
{
    public void Write(string filePath, IReadOnlyCollection<string> messages, string divider)
    {
        var fullPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, string.Join($"{Environment.NewLine}{divider}{Environment.NewLine}", messages), new UTF8Encoding(false));
    }
}
