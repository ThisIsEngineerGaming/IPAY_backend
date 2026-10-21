using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

[TestFixture]
public class OrderStatusHistoryRepositoryTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAndGetByOrderId_ReturnsHistoryForThatOrder()
    {
        var repo = new OrderStatusHistoryRepository(Db);
        var orderId = 84001 + Random.Shared.Next(0, 10000);
        var record = await repo.AddAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            ChangedAt = DateTime.UtcNow,
            Comment = "created"
        });
        try
        {
            var result = await repo.GetByOrderIdAsync(orderId);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(record.Id));
            Assert.That(result[0].Comment, Is.EqualTo("created"));
        }
        finally { await Db.Collection("order_status_history").Document(record.Id.ToString()).DeleteAsync(); }
    }
}
