using AffiliateMessages.Models;

namespace AffiliateMessages.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Product_WithEmptyName_PreservesValidProduct()
    {
        var product = new Product("1", null, 79.19m, "https://afiliado.example");

        Assert.Null(product.Name);
        Assert.Equal(79.19m, product.Price);
        Assert.Equal("https://afiliado.example", product.AffiliateLink);
    }
}
