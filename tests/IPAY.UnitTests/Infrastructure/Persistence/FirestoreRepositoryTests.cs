using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence;

[TestFixture]
public class FirestoreRepositoryTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAsync_AssignsIdAndCanReadEntityBack()
    {
        var collection = UniqueCollection("generic_films");
        var repository = new FirestoreRepository<Film, FilmDocument>(
            Db, collection, FilmDocument.FromEntity, d => d.ToEntity());
        var entity = new Film { Name = "Test film", Year = 2024, ImdbId = Guid.NewGuid().ToString(), GenreIds = new() { 2 } };

        var added = await repository.AddAsync(entity);
        try
        {
            Assert.That(added.Id, Is.GreaterThan(0));
            var loaded = await repository.GetByIdAsync(added.Id);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.Name, Is.EqualTo("Test film"));
            Assert.That(loaded.Year, Is.EqualTo(2024));
        }
        finally { await repository.DeleteAsync(added.Id); }
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNullForMissingDocument()
    {
        var repository = new FirestoreRepository<Film, FilmDocument>(
            Db, UniqueCollection("generic_missing"), FilmDocument.FromEntity, d => d.ToEntity());
        Assert.That(await repository.GetByIdAsync(987654321), Is.Null);
    }

    [Test]
    public async Task UpdateAsync_UsesRequestedDocumentId()
    {
        var collection = UniqueCollection("generic_update");
        var repository = new FirestoreRepository<Film, FilmDocument>(
            Db, collection, FilmDocument.FromEntity, d => d.ToEntity());
        var entity = new Film { Name = "Before" };
        await repository.UpdateAsync(77, entity);
        try
        {
            Assert.That(entity.Id, Is.EqualTo(77));
            Assert.That((await repository.GetByIdAsync(77))!.Name, Is.EqualTo("Before"));
        }
        finally { await repository.DeleteAsync(77); }
    }

    [Test]
    public async Task GetAllAsync_ReturnsStoredDocuments()
    {
        var collection = UniqueCollection("generic_all");
        var repository = new FirestoreRepository<Film, FilmDocument>(
            Db, collection, FilmDocument.FromEntity, d => d.ToEntity());
        var entity = new Film { Name = "Only film" };
        var added = await repository.AddAsync(entity);
        try
        {
            var all = await repository.GetAllAsync();
            Assert.That(all.Select(x => x.Name), Does.Contain("Only film"));
        }
        finally { await repository.DeleteAsync(added.Id); }
    }
}
