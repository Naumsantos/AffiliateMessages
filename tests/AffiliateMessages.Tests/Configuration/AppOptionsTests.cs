using AffiliateMessages.Configuration;

namespace AffiliateMessages.Tests.Configuration;

public class AppOptionsTests
{
    [Fact]
    public void Defaults_UseLocalOllamaAndShopeeOutputFiles()
    {
        var options = new AppOptions();

        Assert.Equal("http://localhost:11434", options.OllamaUrl);
        Assert.Equal("llama3.2:latest", options.OllamaModel);
        Assert.Equal("messages.txt", options.OutputFile);
        Assert.Equal("logs.txt", options.LogFile);
        Assert.Equal("====================", options.Divider);
    }
}
