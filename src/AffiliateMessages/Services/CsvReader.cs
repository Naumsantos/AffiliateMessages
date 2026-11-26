using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using AffiliateMessages.Models;

namespace AffiliateMessages.Services;

public sealed record CsvReadEntry(int LineNumber, Product? Product, string? InvalidReason);

public sealed class CsvReader
{
    public IReadOnlyList<CsvReadEntry> Read(string filePath)
    {
        var entries = new List<CsvReadEntry>();
        using var stream = File.OpenRead(filePath);
        using var textReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            BadDataFound = null,
            MissingFieldFound = null
        };

        using var csv = new CsvHelper.CsvReader(textReader, configuration);
        var hasSkippedHeader = false;

        while (csv.Read())
        {
            var lineNumber = csv.Parser.Row;
            var priceText = csv.GetField(2)?.Trim();
            var link = csv.GetField(8)?.Trim();
            var name = csv.GetField(1)?.Trim();

            if (!hasSkippedHeader)
            {
                var priceLooksLikeHeader = IsHeaderValue(priceText, "preco", "price");
                var nameLooksLikeHeader = IsHeaderValue(name, "nome", "name");
                if (priceLooksLikeHeader || nameLooksLikeHeader)
                {
                    hasSkippedHeader = true;
                    continue;
                }
            }

            if (!PriceParser.TryParse(priceText, out var price))
            {
                entries.Add(new CsvReadEntry(lineNumber, null, "Preco invalido"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(link))
            {
                entries.Add(new CsvReadEntry(lineNumber, null, "Link de afiliado ausente"));
                continue;
            }

            entries.Add(new CsvReadEntry(
                lineNumber,
                new Product(csv.GetField(0)?.Trim(), string.IsNullOrWhiteSpace(name) ? null : name, price, link),
                null));
        }

        return entries;
    }

    private static bool IsHeaderValue(string? value, params string[] terms)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        normalized = Regex.Replace(normalized, @"\p{Mn}", string.Empty);
        return terms.Any(term => normalized.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
