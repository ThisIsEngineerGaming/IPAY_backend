using IPAY.UnitTests.Infrastructure;
using Google.Cloud.Firestore;
using IPAY.Infrastructure.Repositories.Media;
using NUnit.Framework;
using System.Reflection;

namespace IPAY.UnitTests.Infrastructure.Repositories.Media;

[TestFixture]
public class MediaPageQueryTests : FirestoreEmulatorTestBase
{
    private static async Task<Query> BuildAsync(
        CollectionReference collection, int limit, string? lastDocId, int? genreId,
        string? search, string? sortBy, string? sortDir)
    {
        var type = typeof(FilmRepo).Assembly.GetType("IPAY.Infrastructure.Repositories.Media.MediaPageQuery")!;
        var method = type.GetMethod("BuildAsync", BindingFlags.Public | BindingFlags.Static)!;
        return await (Task<Query>)method.Invoke(null, new object?[]
        {
            collection, limit, lastDocId, genreId, search, sortBy, sortDir
        })!;
    }

    [Test]
    public async Task BuildAsync_SearchesByNormalizedNamePrefixAndGenre()
    {
        var collection = Db.Collection(UniqueCollection("page_query_search"));
        await collection.Document("1").SetAsync(new Dictionary<string, object>
        {
            ["NameLower"] = "north shore", ["GenreIds"] = new[] { 7 }, ["Rating"] = 8.0,
            ["Year"] = 2020, ["CreatedAt"] = Timestamp.FromDateTime(DateTime.UtcNow)
        });
        await collection.Document("2").SetAsync(new Dictionary<string, object>
        {
            ["NameLower"] = "south shore", ["GenreIds"] = new[] { 7 }, ["Rating"] = 9.0,
            ["Year"] = 2021, ["CreatedAt"] = Timestamp.FromDateTime(DateTime.UtcNow)
        });
        await collection.Document("3").SetAsync(new Dictionary<string, object>
        {
            ["NameLower"] = "north road", ["GenreIds"] = new[] { 8 }, ["Rating"] = 7.0,
            ["Year"] = 2022, ["CreatedAt"] = Timestamp.FromDateTime(DateTime.UtcNow)
        });

        var query = await BuildAsync(collection, 10, null, 7, " NORTH ", null, null);
        var snapshot = await query.GetSnapshotAsync();

        Assert.That(snapshot.Documents.Select(d => d.Id), Is.EquivalentTo(new[] { "1" }));
    }

    [Test]
    public async Task BuildAsync_SortsByRatingDescendingAndAppliesLimit()
    {
        var collection = Db.Collection(UniqueCollection("page_query_sort"));
        foreach (var item in new[] { ("1", 4.0), ("2", 9.0), ("3", 6.0) })
        {
            await collection.Document(item.Item1).SetAsync(new Dictionary<string, object>
            {
                ["NameLower"] = "item " + item.Item1, ["GenreIds"] = new[] { 1 },
                ["Rating"] = item.Item2, ["Year"] = 2020,
                ["CreatedAt"] = Timestamp.FromDateTime(DateTime.UtcNow)
            });
        }

        var query = await BuildAsync(collection, 2, null, null, null, "rating", "DESC");
        var snapshot = await query.GetSnapshotAsync();

        Assert.That(snapshot.Documents, Has.Count.EqualTo(2));
        Assert.That(snapshot.Documents.Select(d => d.GetValue<double>("Rating")), Is.EqualTo(new[] { 9.0, 6.0 }));
    }
}
