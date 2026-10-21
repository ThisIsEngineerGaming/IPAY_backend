using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Repositories.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Media;

/// <summary>
/// Sorting and cursor paging of MediaPageQuery, exercised through FilmRepo.GetPageAsync.
/// Needs the Firestore emulator (see FirestoreEmulatorTestBase); ignored when it is not running.
/// </summary>
[TestFixture]
public class MediaPageQueryPagingTests : FirestoreEmulatorTestBase
{
    private static DateTime Utc(int year, int month = 1, int day = 1) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private async Task<FilmRepo> SeedAsync(string prefix)
    {
        var repo = new FilmRepo(Db, UniqueCollection(prefix));
        await repo.AddAsync(new Film { Name = "Alpha", Year = 2001, Rating = 5, CreatedAt = Utc(2020, 3, 1), GenreIds = new() { 1 } });
        await repo.AddAsync(new Film { Name = "Bravo", Year = 2003, Rating = 9, CreatedAt = Utc(2020, 1, 1), GenreIds = new() { 2 } });
        await repo.AddAsync(new Film { Name = "Charlie", Year = 2002, Rating = 7, CreatedAt = Utc(2020, 2, 1), GenreIds = new() { 1 } });
        return repo;
    }

    private static string[] Names(IEnumerable<Film> films) => films.Select(f => f.Name).ToArray();

    [Test]
    public async Task SortByYear_AscendingIsTheDefaultDirection()
    {
        var repo = await SeedAsync("media_year");

        var result = await repo.GetPageAsync(10, null, null, null, "year", null);

        Assert.That(Names(result), Is.EqualTo(new[] { "Alpha", "Charlie", "Bravo" }));
    }

    [Test]
    public async Task SortByRating_DescendingIsCaseInsensitive()
    {
        var repo = await SeedAsync("media_rating");

        var result = await repo.GetPageAsync(10, null, null, null, " Rating ", "DESC");

        Assert.That(Names(result), Is.EqualTo(new[] { "Bravo", "Charlie", "Alpha" }));
    }

    [Test]
    public async Task SortByDate_UsesCreationTime()
    {
        var repo = await SeedAsync("media_date");

        var result = await repo.GetPageAsync(10, null, null, null, "date", "asc");

        Assert.That(Names(result), Is.EqualTo(new[] { "Bravo", "Charlie", "Alpha" }));
    }

    [Test]
    public async Task UnknownSortField_FallsBackToDocumentIdOrder()
    {
        var repo = await SeedAsync("media_unknown");

        var result = await repo.GetPageAsync(10, null, null, null, "popularity", "desc");

        Assert.That(Names(result), Is.EqualTo(new[] { "Alpha", "Bravo", "Charlie" }));
    }

    [Test]
    public async Task Search_TakesPrecedenceOverSortBy()
    {
        var repo = await SeedAsync("media_search_sort");

        var result = await repo.GetPageAsync(10, null, null, "", "rating", "desc"); // blank search = no search
        var searched = await repo.GetPageAsync(10, null, null, "b", "rating", "desc");

        Assert.That(Names(result), Is.EqualTo(new[] { "Bravo", "Charlie", "Alpha" }));
        Assert.That(Names(searched), Is.EqualTo(new[] { "Bravo" }));
    }

    [Test]
    public async Task Cursor_ContinuesAfterTheLastDocumentWithoutRepeats()
    {
        var repo = await SeedAsync("media_cursor");

        var first = await repo.GetPageAsync(2, null, null, null, "rating", "desc");
        var second = await repo.GetPageAsync(2, first[^1].Id.ToString(), null, null, "rating", "desc");

        Assert.That(Names(first), Is.EqualTo(new[] { "Bravo", "Charlie" }));
        Assert.That(Names(second), Is.EqualTo(new[] { "Alpha" }));
    }

    [Test]
    public async Task Cursor_PointingAtAMissingDocument_IsIgnored()
    {
        var repo = await SeedAsync("media_missing_cursor");

        var result = await repo.GetPageAsync(10, "999999", null, null, "year", "asc");

        Assert.That(Names(result), Is.EqualTo(new[] { "Alpha", "Charlie", "Bravo" }));
    }

    [Test]
    public async Task GenreFilter_CombinesWithSortingAndLimit()
    {
        var repo = await SeedAsync("media_genre");

        var result = await repo.GetPageAsync(1, null, 1, null, "rating", "desc");

        Assert.That(Names(result), Is.EqualTo(new[] { "Charlie" }));
    }
}
