using AffiliateMessages.Services;

namespace AffiliateMessages.Tests.Services;

public class CsvReaderTests
{
    [Fact]
    public void Read_WithHeaderQuotedCommaAndFixedColumns_ReturnsProduct()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path,
            "id,nome,preco,vendas,loja,taxa,comissao,link produto,link afiliado\n" +
            "1,\"Sandalia, feminina\",\"79,19\",10,Loja,5,3,https://produto.example,https://afiliado.example\n");

        var entries = new CsvReader().Read(path);

        Assert.Single(entries);
        Assert.NotNull(entries[0].Product);
        var product = entries[0].Product!;
        Assert.Equal("Sandalia, feminina", product.Name);
        Assert.Equal(79.19m, product.Price);
        Assert.Equal("https://afiliado.example", product.AffiliateLink);
    }

    [Fact]
    public void Read_WithoutHeader_InvalidPriceIsReportedAndValidLineRemains()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path,
            "1,Produto ruim,invalido,10,Loja,5,3,https://produto.example,https://afiliado.example\n" +
            "2,Produto valido,1700,20,Loja,5,4,https://produto.example,https://afiliado.example/2\n");

        var entries = new CsvReader().Read(path);

        Assert.Equal("Preco invalido", entries[0].InvalidReason);
        Assert.Equal(1700m, entries[1].Product!.Price);
    }
}
