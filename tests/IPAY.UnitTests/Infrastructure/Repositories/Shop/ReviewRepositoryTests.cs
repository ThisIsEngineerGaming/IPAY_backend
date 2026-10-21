using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

[TestFixture]
public class ReviewRepositoryTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAndGetById_RoundTripsReview()
    {
        var repo = new ReviewRepository(Db);
        var review = await repo.AddAsync(new Review { ProductId = 91001, UserId = 91002, Rating = 5, Comment = "Good" });
        try
        {
            var loaded = await repo.GetByIdAsync(review.Id);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.Comment, Is.EqualTo("Good"));
            Assert.That(loaded.Rating, Is.EqualTo(5));
        }
        finally { await repo.DeleteAsync(review.Id); }
    }

    [Test]
    public async Task GetByProductId_ReturnsNewestFirst()
    {
        var repo = new ReviewRepository(Db);
        var productId = 91003 + Random.Shared.Next(0, 10000);
        var old = await repo.AddAsync(new Review { ProductId = productId, Comment = "old", CreatedAt = DateTime.UtcNow.AddDays(-2) });
        var recent = await repo.AddAsync(new Review { ProductId = productId, Comment = "recent", CreatedAt = DateTime.UtcNow });
        try
        {
            var result = await repo.GetByProductIdAsync(productId);
            Assert.That(result.Select(x => x.Comment), Is.EqualTo(new[] { "recent", "old" }));
            Assert.That(await repo.GetByIdAsync(int.MaxValue), Is.Null);
        }
        finally { await repo.DeleteAsync(old.Id); await repo.DeleteAsync(recent.Id); }
    }
}
