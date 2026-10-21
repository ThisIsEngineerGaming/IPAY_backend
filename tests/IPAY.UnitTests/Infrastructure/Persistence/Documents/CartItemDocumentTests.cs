using IPAY.Domain.Entities.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

[TestFixture]
public class CartItemDocumentTests
{
    [Test]
    public void FromEntity_CopiesDocumentedFields()
    {
        var entity = new CartItem { Id = 11, CartId = 12, ProductId = 13, Quantity = 14 };

        var document = CartItemDocument.FromEntity(entity);

        Assert.Multiple(() =>
        {
            Assert.That(document.Id, Is.EqualTo(11));
            Assert.That(document.ProductId, Is.EqualTo(13));
            Assert.That(document.Quantity, Is.EqualTo(14));
        });
    }

    [Test]
    public void ToEntity_CopiesDocumentedFields()
    {
        var document = new CartItemDocument { Id = 21, ProductId = 22, Quantity = 23 };

        var entity = document.ToEntity();

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(21));
            Assert.That(entity.ProductId, Is.EqualTo(22));
            Assert.That(entity.Quantity, Is.EqualTo(23));
        });
    }

    [Test]
    public void RoundTrip_PreservesFieldsRepresentedInDocument()
    {
        var entity = new CartItem { Id = 1, CartId = 2, ProductId = 3, Quantity = 4 };

        var result = CartItemDocument.FromEntity(entity).ToEntity();

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.ProductId, Is.EqualTo(3));
            Assert.That(result.Quantity, Is.EqualTo(4));
        });
        // CartId is intentionally not represented by CartItemDocument; it is supplied by cart nesting.
        Assert.That(result.CartId, Is.EqualTo(0));
    }
}
