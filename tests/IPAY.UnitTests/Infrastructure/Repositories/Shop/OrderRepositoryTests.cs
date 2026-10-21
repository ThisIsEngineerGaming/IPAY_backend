using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

[TestFixture]
public class OrderRepositoryTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAsync_AssignsOrderIdToOrderAndItems()
    {
        var repo = new OrderRepository(Db);
        var order = new Order
        {
            UserId = 83001,
            Address = new Address { Country = "Ukraine", City = "Odesa", Street = "Main", UserId = 83001 },
            Items = new() { new OrderItem { ProductId = 4, Quantity = 2, ProductName = "Widget", Price = 5 } }
        };
        var added = await repo.AddAsync(order);
        try
        {
            Assert.That(added.Id, Is.GreaterThan(0));
            Assert.That(added.Items[0].OrderId, Is.EqualTo(added.Id));
            Assert.That((await repo.GetByIdWithItemsAsync(added.Id))!.Items, Has.Count.EqualTo(1));
        }
        finally { await repo.DeleteAsync(added.Id); }
    }

    [Test]
    public async Task GetByUserId_ReturnsOnlyTheUsersOrders()
    {
        var repo = new OrderRepository(Db);
        var mine = await repo.AddAsync(new Order { UserId = 83002, Address = new Address() });
        var other = await repo.AddAsync(new Order { UserId = 83003, Address = new Address() });
        try
        {
            var result = await repo.GetByUserIdAsync(83002);
            Assert.That(result.Select(x => x.Id), Does.Contain(mine.Id));
            Assert.That(result.Select(x => x.Id), Does.Not.Contain(other.Id));
            Assert.That(await repo.GetByIdAsync(int.MaxValue), Is.Null);
        }
        finally { await repo.DeleteAsync(mine.Id); await repo.DeleteAsync(other.Id); }
    }
}
