using System.Text;
using AffiliateMessages.Models;

namespace AffiliateMessages.Services;

public static class MessageFormatter
{
    public static string Format(Product product, string? phrase)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(phrase))
        {
            lines.Add("");      
            lines.Add(phrase.Trim());
        }
        if (!string.IsNullOrWhiteSpace(product.Name))
        {
            lines.Add("");
            lines.Add(product.Name.Trim());
        }
        lines.Add("");
        lines.Add($"✅ POR : R$ {PriceFormatter.Format(product.Price)}");
        lines.Add("");
        lines.Add(product.AffiliateLink);
        lines.Add("");

        return string.Join(Environment.NewLine, lines);
    }
}
