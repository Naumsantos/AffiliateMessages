using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace AffiliateMessages.Services;

public static class PhraseValidator
{
    public static bool TryValidate(string? phrase, out string? validatedPhrase)
    {
        validatedPhrase = null;
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return false;
        }

        var trimmed = RemoveEnclosingQuotes(phrase.Trim());
        if (trimmed.Length == 0)
        {
            return false;
        }
        var emojiCount = 0;
        foreach (var rune in trimmed.EnumerateRunes())
        {
            var value = rune.Value;
            if ((value >= 0x2600 && value <= 0x27BF)
                || (value >= 0x1F300 && value <= 0x1FAFF))
            {
                emojiCount++;
            }
        }
        var words = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Count(word => Regex.IsMatch(word, @"[\p{L}\p{N}]"));

        if (words > 7 || emojiCount > 1)
        {
            return false;
        }

        validatedPhrase = trimmed;
        return true;
    }

    private static string RemoveEnclosingQuotes(string phrase)
    {
        if (phrase.Length < 2)
        {
            return phrase;
        }

        var first = phrase[0];
        var last = phrase[^1];
        var matchingQuote = (first, last) switch
        {
            ('"', '"') or ('\'', '\'') or ('“', '”') or ('‘', '’') or ('«', '»') => true,
            _ => false
        };

        return matchingQuote ? phrase[1..^1].Trim() : phrase;
    }
}
