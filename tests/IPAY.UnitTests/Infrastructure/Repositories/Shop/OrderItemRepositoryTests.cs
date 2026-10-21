using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

[TestFixture]
public class OrderItemRepositoryTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAndGetByOrderId_ReturnsOnlyMatchingItems()
    {
        var repo = new OrderItemRepository(Db);
        var first = await repo.AddAsync(new OrderItem { OrderId = 82001, ProductId = 1, Quantity = 2, ProductName = "A", Price = 10 });
        var second = await repo.AddAsync(new OrderItem { OrderId = 82002, ProductId = 2, Quantity = 1, ProductName = "B", Price = 20 });
        try
        {
            var result = await repo.GetByOrderIdAsync(82001);
            Assert.That(result.Select(x => x.Id), Does.Contain(first.Id));
            Assert.That(result.Select(x => x.Id), Does.Not.Contain(second.Id));
            Assert.That((await repo.GetByIdAsync(first.Id))!.Quantity, Is.EqualTo(2));
        }
        finally { await repo.DeleteAsync(first.Id); await repo.DeleteAsync(second.Id); }
    }

    [Test]
    public async Task UpdateAsync_OverwritesEntityIdWithRequestedId()
    {
        var repo = new OrderItemRepository(Db);
        var item = new OrderItem { OrderId = 82003, ProductId = 3, Quantity = 1, ProductName = "Before" };
        await repo.UpdateAsync(998877, item);
        try
        {
            Assert.That(item.Id, Is.EqualTo(998877));
            Assert.That((await repo.GetByIdAsync(998877))!.ProductName, Is.EqualTo("Before"));
        }
        finally { await repo.DeleteAsync(998877); }
    }
}
