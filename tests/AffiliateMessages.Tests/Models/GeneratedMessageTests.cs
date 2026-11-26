using AffiliateMessages.Models;

namespace AffiliateMessages.Tests.Models;

public class GeneratedMessageTests
{
    [Fact]
    public void GeneratedMessage_StoresOptionalPhraseAndFormattedPrice()
    {
        var message = new GeneratedMessage(null, null, "R$ 1.700", "https://afiliado.example");

        Assert.Null(message.Phrase);
        Assert.Null(message.Name);
        Assert.Equal("R$ 1.700", message.PriceText);
    }
}
