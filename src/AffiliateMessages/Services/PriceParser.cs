using System.Globalization;

namespace AffiliateMessages.Services;

public static class PriceParser
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static bool TryParse(string? value, out decimal price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (decimal.TryParse(normalized, NumberStyles.Number, BrazilianCulture, out price))
        {
            return price >= 0;
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out price)
            && price >= 0;
    }
}
