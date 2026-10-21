using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Repositories.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Media;

/// <summary>
/// Series links, idempotency and "missing genre is a no-op" behaviour of GenreRepo.
/// Needs the Firestore emulator (see FirestoreEmulatorTestBase); ignored when it is not running.
/// </summary>
[TestFixture]
public class GenreRepoSeriesTests : FirestoreEmulatorTestBase
{
    private const int MissingGenreId = 987654;

    [Test]
    public async Task AddAndRemoveSeries_UpdatesGenreSerialIds()
    {
        var repo = new GenreRepo(Db, UniqueCollection("genres_series"));
        var genre = await repo.AddAsync(new Genre { Name = "drama" });

        await repo.AddSeriesAsync(genre.Id, 31);
        Assert.That((await repo.GetByIdAsync(genre.Id))!.SerialIds, Is.EqualTo(new[] { 31 }));

        await repo.RemoveSeriesAsync(genre.Id, 31);
        Assert.That((await repo.GetByIdAsync(genre.Id))!.SerialIds, Is.Empty);
    }

    [Test]
    public async Task AddFilm_TwiceWithTheSameId_DoesNotDuplicateIt()
    {
        var repo = new GenreRepo(Db, UniqueCollection("genres_dupes"));
        var genre = await repo.AddAsync(new Genre { Name = "comedy" });

        await repo.AddFilmAsync(genre.Id, 7);
        await repo.AddFilmAsync(genre.Id, 7);

        Assert.That((await repo.GetByIdAsync(genre.Id))!.FilmIds, Is.EqualTo(new[] { 7 }));
    }

    [Test]
    public async Task FilmAndSeriesLinks_AreIndependentLists()
    {
        var repo = new GenreRepo(Db, UniqueCollection("genres_independent"));
        var genre = await repo.AddAsync(new Genre { Name = "thriller" });

        await repo.AddFilmAsync(genre.Id, 1);
        await repo.AddSeriesAsync(genre.Id, 2);
        await repo.RemoveFilmAsync(genre.Id, 1);

        var loaded = await repo.GetByIdAsync(genre.Id);
        Assert.That(loaded!.FilmIds, Is.Empty);
        Assert.That(loaded.SerialIds, Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public async Task OperationsOnAMissingGenre_DoNotThrow()
    {
        var repo = new GenreRepo(Db, UniqueCollection("genres_missing"));

        Assert.Multiple(() =>
        {
            Assert.DoesNotThrowAsync(() => repo.AddFilmAsync(MissingGenreId, 1));
            Assert.DoesNotThrowAsync(() => repo.RemoveFilmAsync(MissingGenreId, 1));
            Assert.DoesNotThrowAsync(() => repo.AddSeriesAsync(MissingGenreId, 1));
            Assert.DoesNotThrowAsync(() => repo.RemoveSeriesAsync(MissingGenreId, 1));
            Assert.DoesNotThrowAsync(() => repo.ClearAsync(MissingGenreId));
        });
        Assert.That(await repo.GetByIdAsync(MissingGenreId), Is.Null, "a no-op must not create the genre");
    }
}
