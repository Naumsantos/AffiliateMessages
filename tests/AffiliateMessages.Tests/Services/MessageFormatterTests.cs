using AffiliateMessages.Models;
using AffiliateMessages.Services;

namespace AffiliateMessages.Tests.Services;

public class MessageFormatterTests
{
    [Fact]
    public void Format_WithPhraseAndName_ReturnsExpectedLines()
    {
        var product = new Product("1", "Sandalia", 79.19m, "https://affiliate.example/item");
        var message = MessageFormatter.Format(product, "SEUS PES VÃO BRILHAR 😘");

        Assert.Equal(
            $"{Environment.NewLine}SEUS PES VÃO BRILHAR 😘{Environment.NewLine}{Environment.NewLine}" +
            $"Sandalia{Environment.NewLine}{Environment.NewLine}✅ POR : R$ 79,19{Environment.NewLine}{Environment.NewLine}" +
            $"https://affiliate.example/item{Environment.NewLine}",
            message);
        Assert.Contains("Sandalia", message);
        Assert.Contains("✅ POR : R$ 79,19", message);
    }

    [Fact]
    public void Format_WithEmptyName_OmitsNameLine()
    {
        var product = new Product("1", null, 1700m, "https://affiliate.example/item");
        var message = MessageFormatter.Format(product, null);

        Assert.DoesNotContain("Sandalia", message);
        Assert.Contains($"{Environment.NewLine}✅ POR : R$ 1.700", message);
    }
}
