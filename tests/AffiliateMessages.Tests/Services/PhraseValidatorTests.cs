using AffiliateMessages.Services;

namespace AffiliateMessages.Tests.Services;

public class PhraseValidatorTests
{
    [Fact]
    public void Validate_SevenWordsAndOneEmoji_ReturnsValid()
    {
        Assert.True(PhraseValidator.TryValidate("SUA COZINHA VAI FICAR CHIQUE C/ ESSE 😘", out var phrase));
        Assert.Equal("SUA COZINHA VAI FICAR CHIQUE C/ ESSE 😘", phrase);
    }

    [Fact]
    public void Validate_QuotedPhrase_RemovesEnclosingQuotes()
    {
        Assert.True(PhraseValidator.TryValidate("\"SUA COZINHA FICARÁ LINDA 😘\"", out var phrase));
        Assert.Equal("SUA COZINHA FICARÁ LINDA 😘", phrase);
    }

    [Theory]
    [InlineData("uma frase com oito palavras neste produto agora")]
    [InlineData("Produto lindo 😘 demais 😍")]
    public void Validate_InvalidPhrase_ReturnsFalse(string input)
    {
        Assert.False(PhraseValidator.TryValidate(input, out _));
    }
}
