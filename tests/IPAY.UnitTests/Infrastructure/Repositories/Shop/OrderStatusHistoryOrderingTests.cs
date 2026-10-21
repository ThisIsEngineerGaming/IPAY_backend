using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

/// <summary>
/// Needs the Firestore emulator (see FirestoreEmulatorTestBase); ignored when it is not running.
/// OrderStatusHistoryRepository uses the fixed collection "order_status_history", so each test uses its own
/// order id and removes the records it created.
/// </summary>
[TestFixture]
public class OrderStatusHistoryOrderingTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task GetByOrderId_ReturnsRecordsOldestFirstAndIgnoresOtherOrders()
    {
        var repo = new OrderStatusHistoryRepository(Db);
        var orderId = 850000 + Random.Shared.Next(0, 100000);
        var now = DateTime.UtcNow;

        var later = await repo.AddAsync(new OrderStatusHistory { OrderId = orderId, ChangedAt = now, Comment = "later" });
        var earlier = await repo.AddAsync(new OrderStatusHistory { OrderId = orderId, ChangedAt = now.AddHours(-3), Comment = "earlier" });
        var other = await repo.AddAsync(new OrderStatusHistory { OrderId = orderId + 1, ChangedAt = now, Comment = "other order" });
        try
        {
            var result = await repo.GetByOrderIdAsync(orderId);

            Assert.That(result.Select(r => r.Comment), Is.EqualTo(new[] { "earlier", "later" }));
            Assert.That(later.Id, Is.Not.EqualTo(earlier.Id), "each record gets its own id");
            Assert.That(await repo.GetByOrderIdAsync(orderId + 2), Is.Empty);
        }
        finally
        {
            foreach (var record in new[] { later, earlier, other })
                await Db.Collection("order_status_history").Document(record.Id.ToString()).DeleteAsync();
        }
    }

    [Test]
    public async Task AddAsync_AssignsIncreasingIds()
    {
        var repo = new OrderStatusHistoryRepository(Db);
        var orderId = 950000 + Random.Shared.Next(0, 100000);

        var first = await repo.AddAsync(new OrderStatusHistory { OrderId = orderId, ChangedAt = DateTime.UtcNow });
        var second = await repo.AddAsync(new OrderStatusHistory { OrderId = orderId, ChangedAt = DateTime.UtcNow });
        try
        {
            Assert.That(second.Id, Is.EqualTo(first.Id + 1));
        }
        finally
        {
            await Db.Collection("order_status_history").Document(first.Id.ToString()).DeleteAsync();
            await Db.Collection("order_status_history").Document(second.Id.ToString()).DeleteAsync();
        }
    }
}
