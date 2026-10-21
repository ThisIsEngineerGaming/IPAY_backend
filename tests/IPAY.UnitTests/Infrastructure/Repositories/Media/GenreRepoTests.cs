using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Repositories.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Media;

[TestFixture]
public class GenreRepoTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAndRemoveFilm_UpdatesGenreFilmIds()
    {
        var repo = new GenreRepo(Db, UniqueCollection("genres"));
        var genre = await repo.AddAsync(new Genre { Name = "test genre" });
        try
        {
            await repo.AddFilmAsync(genre.Id, 21);
            Assert.That((await repo.GetByIdAsync(genre.Id))!.FilmIds, Does.Contain(21));
            await repo.RemoveFilmAsync(genre.Id, 21);
            Assert.That((await repo.GetByIdAsync(genre.Id))!.FilmIds, Does.Not.Contain(21));
        }
        finally { await repo.DeleteAsync(genre.Id); }
    }

    [Test]
    public async Task ClearAsync_EmptiesFilmAndSeriesIds()
    {
        var repo = new GenreRepo(Db, UniqueCollection("genres_clear"));
        var genre = await repo.AddAsync(new Genre { Name = "test", FilmIds = new() { 1 }, SerialIds = new() { 2 } });
        try
        {
            await repo.ClearAsync(genre.Id);
            var cleared = await repo.GetByIdAsync(genre.Id);
            Assert.That(cleared!.FilmIds, Is.Empty);
            Assert.That(cleared.SerialIds, Is.Empty);
        }
        finally { await repo.DeleteAsync(genre.Id); }
    }
}
