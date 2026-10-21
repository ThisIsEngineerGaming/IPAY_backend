using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

[TestFixture]
public class ProductRepoTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddGetUpdateDelete_RoundTripsProduct()
    {
        var repo = new ProductRepo(Db, UniqueCollection("products_crud"));
        var product = await repo.AddProductAsync(new Product { Name = "Headphones", Price = 100, SellerId = "seller-1", Manufacturer = "Acme", CategoryId = 3, CreatedAt = DateTime.UtcNow });
        try
        {
            Assert.That((await repo.GetProductByIdAsync(product.Id))!.Name, Is.EqualTo("Headphones"));
            await repo.UpdateProductAsync(product.Id, new Product { Name = "Updated", Price = 90 });
            Assert.That((await repo.GetProductByIdAsync(product.Id))!.Name, Is.EqualTo("Updated"));
        }
        finally { await repo.DeleteProductAsync(product.Id); }
        Assert.That(await repo.GetProductByIdAsync(product.Id), Is.Null);
    }

    [Test]
    public async Task GetBySeller_ReturnsSellerProductsNewestFirst()
    {
        var collection = UniqueCollection("products_seller");
        var repo = new ProductRepo(Db, collection);
        var old = await repo.AddProductAsync(new Product { Name = "Old", SellerId = "s1", CreatedAt = DateTime.UtcNow.AddDays(-2) });
        var recent = await repo.AddProductAsync(new Product { Name = "Recent", SellerId = "s1", CreatedAt = DateTime.UtcNow });
        var other = await repo.AddProductAsync(new Product { Name = "Other", SellerId = "s2" });
        try
        {
            var result = await repo.GetBySellerAsync("s1");
            Assert.That(result.Select(x => x.Name), Is.EqualTo(new[] { "Recent", "Old" }));
            Assert.That(result.Select(x => x.Name), Does.Not.Contain("Other"));
        }
        finally { await repo.DeleteProductAsync(old.Id); await repo.DeleteProductAsync(recent.Id); await repo.DeleteProductAsync(other.Id); }
    }

    [Test]
    public async Task GetFilteredAsync_FiltersCategoryAndPrice()
    {
        var collection = UniqueCollection("products_filter");
        var repo = new ProductRepo(Db, collection);
        var included = await repo.AddProductAsync(new Product { Name = "Lamp", CategoryId = 4, Price = 25, Manufacturer = "Acme" });
        var wrongCategory = await repo.AddProductAsync(new Product { Name = "Table", CategoryId = 5, Price = 30, Manufacturer = "Acme" });
        var expensive = await repo.AddProductAsync(new Product { Name = "Chair", CategoryId = 4, Price = 200, Manufacturer = "Acme" });
        try
        {
            var result = await repo.GetFilteredAsync(20, null, 4, 10, 50, null, null);
            Assert.That(result.Select(x => x.Id), Does.Contain(included.Id));
            Assert.That(result.Select(x => x.Id), Does.Not.Contain(wrongCategory.Id));
            Assert.That(result.Select(x => x.Id), Does.Not.Contain(expensive.Id));
        }
        finally { await repo.DeleteProductAsync(included.Id); await repo.DeleteProductAsync(wrongCategory.Id); await repo.DeleteProductAsync(expensive.Id); }
    }

    [Test]
    public async Task GetLimitedProduct_RespectsLimitAndCursor()
    {
        var collection = UniqueCollection("products_paging");
        var repo = new ProductRepo(Db, collection);
        var products = new List<Product>();
        for (var i = 0; i < 3; i++)
            products.Add(await repo.AddProductAsync(new Product { Name = "Product " + i }));
        try
        {
            var first = await repo.GetLimitedProduct(2, null);
            Assert.That(first, Has.Count.EqualTo(2));
            var second = await repo.GetLimitedProduct(2, first[^1].Id.ToString());
            Assert.That(second.Select(x => x.Id).Intersect(first.Select(x => x.Id)), Is.Empty);
        }
        finally { foreach (var product in products) await repo.DeleteProductAsync(product.Id); }
    }

    [Test]
    public async Task GetFilteredAsync_SearchesCaseInsensitivelyByNamePrefix()
    {
        var collection = UniqueCollection("products_search");
        var repo = new ProductRepo(Db, collection);
        var match = await repo.AddProductAsync(new Product { Name = "Wireless Mouse", CategoryId = 1, Price = 20 });
        var noMatch = await repo.AddProductAsync(new Product { Name = "Keyboard", CategoryId = 1, Price = 30 });
        try
        {
            var result = await repo.GetFilteredAsync(10, null, null, null, null, null, " WIRE ");
            Assert.That(result.Select(x => x.Id), Does.Contain(match.Id));
            Assert.That(result.Select(x => x.Id), Does.Not.Contain(noMatch.Id));
        }
        finally { await repo.DeleteProductAsync(match.Id); await repo.DeleteProductAsync(noMatch.Id); }
    }

    [Test]
    public async Task GetSortedAsync_SortsByPriceDescending()
    {
        var collection = UniqueCollection("products_sorted");
        var repo = new ProductRepo(Db, collection);
        var cheap = await repo.AddProductAsync(new Product { Name = "Cheap", Price = 5 });
        var middle = await repo.AddProductAsync(new Product { Name = "Middle", Price = 15 });
        var expensive = await repo.AddProductAsync(new Product { Name = "Expensive", Price = 25 });
        try
        {
            var result = await repo.GetSortedAsync(3, null, "price", "desc");
            Assert.That(result.Select(x => x.Name), Is.EqualTo(new[] { "Expensive", "Middle", "Cheap" }));
        }
        finally
        {
            await repo.DeleteProductAsync(cheap.Id);
            await repo.DeleteProductAsync(middle.Id);
            await repo.DeleteProductAsync(expensive.Id);
        }
    }

}
