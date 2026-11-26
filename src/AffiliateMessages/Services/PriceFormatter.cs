using System.Globalization;

namespace AffiliateMessages.Services;

public static class PriceFormatter
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static string Format(decimal price)
    {
        var pattern = decimal.Truncate(price) == price ? "#,##0" : "#,##0.00";
        return price.ToString(pattern, BrazilianCulture);
    }
}
