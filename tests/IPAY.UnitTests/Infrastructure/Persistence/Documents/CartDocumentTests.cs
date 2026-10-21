using IPAY.Domain.Entities.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

[TestFixture]
public class CartDocumentTests
{
    [Test]
    public void FromEntity_CopiesIdsAndMapsEveryItem()
    {
        var cart = new Cart
        {
            Id = 12,
            UserId = 34,
            Items =
            [
                new CartItem { Id = 1, CartId = 12, ProductId = 101, Quantity = 2 },
                new CartItem { Id = 2, CartId = 12, ProductId = 202, Quantity = 5 }
            ]
        };

        var document = CartDocument.FromEntity(cart);

        Assert.Multiple(() =>
        {
            Assert.That(document.Id, Is.EqualTo(12));
            Assert.That(document.UserId, Is.EqualTo(34));
            Assert.That(document.Items, Has.Count.EqualTo(2));
            Assert.That(document.Items[0].ProductId, Is.EqualTo(101));
            Assert.That(document.Items[0].Quantity, Is.EqualTo(2));
            Assert.That(document.Items[1].ProductId, Is.EqualTo(202));
            Assert.That(document.Items[1].Quantity, Is.EqualTo(5));
        });
    }

    [Test]
    public void ToEntity_CopiesIdsAndMapsEveryItem()
    {
        var document = new CartDocument
        {
            Id = 8,
            UserId = 9,
            Items =
            [
                new CartItemDocument { Id = 3, ProductId = 44, Quantity = 6 },
                new CartItemDocument { Id = 4, ProductId = 55, Quantity = 7 }
            ]
        };

        var entity = document.ToEntity();

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(8));
            Assert.That(entity.UserId, Is.EqualTo(9));
            Assert.That(entity.Items, Has.Count.EqualTo(2));
            Assert.That(entity.Items[0].Id, Is.EqualTo(3));
            Assert.That(entity.Items[0].ProductId, Is.EqualTo(44));
            Assert.That(entity.Items[0].Quantity, Is.EqualTo(6));
            Assert.That(entity.Items[1].Id, Is.EqualTo(4));
            Assert.That(entity.Items[1].ProductId, Is.EqualTo(55));
            Assert.That(entity.Items[1].Quantity, Is.EqualTo(7));
        });
    }

    [Test]
    public void DefaultDocument_HasEmptyItemsCollection()
    {
        var document = new CartDocument();
        Assert.That(document.Items, Is.Not.Null.And.Empty);
    }

    [Test]
    public void RoundTrip_PreservesCartAndItemFieldsRepresentedInDocument()
    {
        var original = new Cart
        {
            Id = 10,
            UserId = 20,
            Items = [new CartItem { Id = 30, CartId = 10, ProductId = 40, Quantity = 3 }]
        };

        var result = CartDocument.FromEntity(original).ToEntity();

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(original.Id));
            Assert.That(result.UserId, Is.EqualTo(original.UserId));
            Assert.That(result.Items.Single().Id, Is.EqualTo(30));
            Assert.That(result.Items.Single().ProductId, Is.EqualTo(40));
            Assert.That(result.Items.Single().Quantity, Is.EqualTo(3));
        });
    }
}
