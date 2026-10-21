using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

/// <summary>
/// Filtering, sorting and cursor paging of ProductRepo (the branches ProductRepoTests does not reach).
/// Needs the Firestore emulator (see FirestoreEmulatorTestBase); ignored when it is not running.
/// </summary>
[TestFixture]
public class ProductRepoPagingTests : FirestoreEmulatorTestBase
{
    private static DateTime Utc(int month) => new(2024, month, 1, 0, 0, 0, DateTimeKind.Utc);

    private static string[] Names(IEnumerable<Product> products) => products.Select(p => p.Name).ToArray();

    private async Task<ProductRepo> SeedAsync(string prefix)
    {
        var repo = new ProductRepo(Db, UniqueCollection(prefix));
        await repo.AddProductAsync(new Product { Name = "Cable", Price = 10, Rating = 3, Manufacturer = "Acme", CategoryId = 1, CreatedAt = Utc(3) });
        await repo.AddProductAsync(new Product { Name = "Lamp", Price = 20, Rating = 5, Manufacturer = "Globex", CategoryId = 1, CreatedAt = Utc(1) });
        await repo.AddProductAsync(new Product { Name = "Desk", Price = 30, Rating = 4, Manufacturer = "Acme", CategoryId = 2, CreatedAt = Utc(2) });
        return repo;
    }

    // ---- GetLimitedProduct ----

    [Test]
    public async Task GetLimitedProduct_WithUnknownCursor_StartsFromTheBeginning()
    {
        var repo = await SeedAsync("products_limited");

        var result = await repo.GetLimitedProduct(2, "999999");

        Assert.That(Names(result), Is.EqualTo(new[] { "Cable", "Lamp" }));
    }

    // ---- GetFilteredAsync ----

    [Test]
    public async Task GetFilteredAsync_FiltersByBrandIgnoringSurroundingSpaces()
    {
        var repo = await SeedAsync("products_brand");

        var result = await repo.GetFilteredAsync(10, null, null, null, null, " Acme ", null);

        Assert.That(Names(result), Is.EqualTo(new[] { "Cable", "Desk" }));
    }

    [Test]
    public async Task GetFilteredAsync_PriceRangeCursorContinuesWithoutRepeats()
    {
        var repo = await SeedAsync("products_price_cursor");

        var first = await repo.GetFilteredAsync(2, null, null, 5, 100, null, null);
        var second = await repo.GetFilteredAsync(2, first[^1].Id.ToString(), null, 5, 100, null, null);

        Assert.That(Names(first), Is.EqualTo(new[] { "Cable", "Lamp" }));
        Assert.That(Names(second), Is.EqualTo(new[] { "Desk" }));
    }

    [Test]
    public async Task GetFilteredAsync_MaxPriceOnly_ExcludesMoreExpensiveProducts()
    {
        var repo = await SeedAsync("products_max_price");

        var result = await repo.GetFilteredAsync(10, null, null, null, 20, null, null);

        Assert.That(Names(result), Is.EqualTo(new[] { "Cable", "Lamp" }));
    }

    [Test]
    public async Task GetFilteredAsync_SearchCursorContinuesInNameOrder()
    {
        var repo = new ProductRepo(Db, UniqueCollection("products_search_cursor"));
        foreach (var name in new[] { "Disk A", "Disk B", "Disk C", "Monitor" })
            await repo.AddProductAsync(new Product { Name = name, Price = 1 });

        var first = await repo.GetFilteredAsync(2, null, null, null, null, null, "disk");
        var second = await repo.GetFilteredAsync(2, first[^1].Id.ToString(), null, null, null, null, "disk");

        Assert.That(Names(first), Is.EqualTo(new[] { "Disk A", "Disk B" }));
        Assert.That(Names(second), Is.EqualTo(new[] { "Disk C" }));
    }

    [Test]
    public async Task GetFilteredAsync_WithoutFilters_PagesByDocumentId()
    {
        var repo = await SeedAsync("products_id_cursor");

        var first = await repo.GetFilteredAsync(2, null, null, null, null, null, null);
        var second = await repo.GetFilteredAsync(2, first[^1].Id.ToString(), null, null, null, null, null);

        Assert.That(Names(first), Is.EqualTo(new[] { "Cable", "Lamp" }));
        Assert.That(Names(second), Is.EqualTo(new[] { "Desk" }));
    }

    [Test]
    public async Task GetFilteredAsync_UnknownCursor_IsIgnored()
    {
        var repo = await SeedAsync("products_unknown_cursor");

        var result = await repo.GetFilteredAsync(10, "999999", null, null, null, null, null);

        Assert.That(result, Has.Count.EqualTo(3));
    }

    // ---- GetSortedAsync ----

    [Test]
    public async Task GetSortedAsync_SortsByRatingDescending()
    {
        var repo = await SeedAsync("products_sort_rating");

        var result = await repo.GetSortedAsync(10, null, "rating", "desc");

        Assert.That(Names(result), Is.EqualTo(new[] { "Lamp", "Desk", "Cable" }));
    }

    [Test]
    public async Task GetSortedAsync_UnknownFieldSortsByPriceAscending()
    {
        var repo = await SeedAsync("products_sort_default");

        var result = await repo.GetSortedAsync(10, null, "colour", "asc");

        Assert.That(Names(result), Is.EqualTo(new[] { "Cable", "Lamp", "Desk" }));
    }

    [Test]
    public async Task GetSortedAsync_DateSortWithCursor_PagesOldestToNewest()
    {
        var repo = await SeedAsync("products_sort_date");

        var first = await repo.GetSortedAsync(2, null, "date", "asc");
        var second = await repo.GetSortedAsync(2, first[^1].Id.ToString(), "date", "asc");

        Assert.That(Names(first), Is.EqualTo(new[] { "Lamp", "Desk" }));
        Assert.That(Names(second), Is.EqualTo(new[] { "Cable" }));
    }

    [Test]
    public async Task GetSortedAsync_PriceDescendingWithCursor_ContinuesWithoutRepeats()
    {
        var repo = await SeedAsync("products_sort_price_cursor");

        var first = await repo.GetSortedAsync(2, null, "price", "desc");
        var second = await repo.GetSortedAsync(2, first[^1].Id.ToString(), "price", "desc");

        Assert.That(Names(first), Is.EqualTo(new[] { "Desk", "Lamp" }));
        Assert.That(Names(second), Is.EqualTo(new[] { "Cable" }));
    }
}
