using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Repositories.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Media;

[TestFixture]
public class FilmRepoTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task GetByImdbId_ReturnsMatchingFilmAndNullForUnknownId()
    {
        var collection = UniqueCollection("films_imdb");
        var repo = new FilmRepo(Db, collection);
        var imdb = "tt" + Guid.NewGuid().ToString("N")[..10];
        var film = await repo.AddAsync(new Film { Name = "IMDb test", ImdbId = imdb });
        try
        {
            Assert.That((await repo.GetByImdbIdAsync(imdb))!.Name, Is.EqualTo("IMDb test"));
            Assert.That(await repo.GetByImdbIdAsync("missing-" + imdb), Is.Null);
        }
        finally { await repo.DeleteAsync(film.Id); }
    }

    [Test]
    public async Task GetPageAsync_AppliesPrefixSearchAndLimit()
    {
        var collection = UniqueCollection("films_page");
        var repo = new FilmRepo(Db, collection);
        var a = await repo.AddAsync(new Film { Name = "Alpha", GenreIds = new() { 5 }, Rating = 8 });
        var b = await repo.AddAsync(new Film { Name = "Alpine", GenreIds = new() { 5 }, Rating = 7 });
        var c = await repo.AddAsync(new Film { Name = "Beta", GenreIds = new() { 5 }, Rating = 9 });
        try
        {
            var result = await repo.GetPageAsync(10, null, 5, " al ", null, null);
            Assert.That(result.Select(x => x.Name), Does.Contain("Alpha"));
            Assert.That(result.Select(x => x.Name), Does.Contain("Alpine"));
            Assert.That(result.Select(x => x.Name), Does.Not.Contain("Beta"));
        }
        finally { await repo.DeleteAsync(a.Id); await repo.DeleteAsync(b.Id); await repo.DeleteAsync(c.Id); }
    }
}
