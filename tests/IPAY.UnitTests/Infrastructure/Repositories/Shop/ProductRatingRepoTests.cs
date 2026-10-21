using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

[TestFixture]
public class ProductRatingRepoTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task RateAsync_UpdatesAverageAndRejectsDuplicateRating()
    {
        var products = UniqueCollection("rated_products");
        var ratings = UniqueCollection("ratings");
        var productRepo = new ProductRepo(Db, products);
        var ratingRepo = new ProductRatingRepo(Db, products, ratings);
        var product = await productRepo.AddProductAsync(new Product { Name = "Rate me", SellerId = "seller", Rating = 4.5, RatingCount = 0 });
        try
        {
            var first = await ratingRepo.RateAsync(product.Id, "buyer-1", 4);
            Assert.That(first.Result, Is.EqualTo(RateResult.Rated));
            Assert.That(first.Rating, Is.EqualTo(4));
            Assert.That(first.RatingCount, Is.EqualTo(1));

            var duplicate = await ratingRepo.RateAsync(product.Id, "buyer-1", 1);
            Assert.That(duplicate.Result, Is.EqualTo(RateResult.AlreadyRated));
            Assert.That(duplicate.RatingCount, Is.EqualTo(1));

            var other = await ratingRepo.RateAsync(product.Id, "buyer-2", 2);
            Assert.That(other.Result, Is.EqualTo(RateResult.Rated));
            Assert.That(other.Rating, Is.EqualTo(3));
            Assert.That(other.RatingCount, Is.EqualTo(2));
        }
        finally
        {
            await productRepo.DeleteProductAsync(product.Id);
            var snapshot = await Db.Collection(ratings).GetSnapshotAsync();
            foreach (var doc in snapshot.Documents) await doc.Reference.DeleteAsync();
        }
    }

    [Test]
    public async Task RateAsync_RejectsMissingAndOwnProducts()
    {
        var products = UniqueCollection("rated_products");
        var ratings = UniqueCollection("ratings");
        var productRepo = new ProductRepo(Db, products);
        var ratingRepo = new ProductRatingRepo(Db, products, ratings);
        var product = await productRepo.AddProductAsync(new Product { Name = "Own", SellerId = "seller" });
        try
        {
            Assert.That((await ratingRepo.RateAsync(int.MaxValue, "buyer", 5)).Result, Is.EqualTo(RateResult.ProductNotFound));
            Assert.That((await ratingRepo.RateAsync(product.Id, "seller", 5)).Result, Is.EqualTo(RateResult.OwnProduct));
        }
        finally { await productRepo.DeleteProductAsync(product.Id); }
    }

    [Test]
    public async Task GetByUserAsync_ReturnsProductToRatingMap()
    {
        var products = UniqueCollection("rated_products");
        var ratings = UniqueCollection("ratings");
        var productRepo = new ProductRepo(Db, products);
        var ratingRepo = new ProductRatingRepo(Db, products, ratings);
        var product = await productRepo.AddProductAsync(new Product { Name = "Map", SellerId = "seller" });
        try
        {
            await ratingRepo.RateAsync(product.Id, "buyer-map", 5);
            var result = await ratingRepo.GetByUserAsync("buyer-map");
            Assert.That(result[product.Id], Is.EqualTo(5));
        }
        finally
        {
            await productRepo.DeleteProductAsync(product.Id);
            var snapshot = await Db.Collection(ratings).GetSnapshotAsync();
            foreach (var doc in snapshot.Documents) await doc.Reference.DeleteAsync();
        }
    }
}
