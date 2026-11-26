using AffiliateMessages.Services;

namespace AffiliateMessages.Tests.Services;

public class PriceFormatterTests
{
    [Theory]
    [InlineData("79,19", "79,19")]
    [InlineData("1700", "1.700")]
    [InlineData("1700,50", "1.700,50")]
    public void Format_PtBrPrice_ReturnsExpectedText(string input, string expected)
    {
        Assert.True(PriceParser.TryParse(input, out var price));
        Assert.Equal(expected, PriceFormatter.Format(price));
    }

    [Fact]
    public void TryParse_InvalidPrice_ReturnsFalse()
    {
        Assert.False(PriceParser.TryParse("inválido", out _));
    }
}
