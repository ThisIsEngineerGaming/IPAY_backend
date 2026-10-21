using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence;

/// <summary>Needs the Firestore emulator (see FirestoreEmulatorTestBase); ignored when it is not running.</summary>
[TestFixture]
public class FirestoreIdGeneratorTests : FirestoreEmulatorTestBase
{
    private FirestoreRepository<Film, FilmDocument> NewRepository(string collection) =>
        new(Db, collection, FilmDocument.FromEntity, d => d.ToEntity());

    [Test]
    public async Task FirstInsert_SeedsCounterAndAssignsPositiveId()
    {
        var repo = NewRepository(UniqueCollection("id_generator"));

        var first = await repo.AddAsync(new Film { Name = "counter test" });
        var second = await repo.AddAsync(new Film { Name = "second" });

        Assert.That(first.Id, Is.EqualTo(1));
        Assert.That(second.Id, Is.EqualTo(first.Id + 1));
    }

    [Test]
    public async Task FirstInsert_IntoCollectionWithExistingData_ContinuesAfterTheHighestId()
    {
        var repo = NewRepository(UniqueCollection("id_generator_seed"));
        await repo.UpdateAsync(5, new Film { Name = "pre-existing" }); // document "5" exists, but no counter yet

        var added = await repo.AddAsync(new Film { Name = "new" });

        Assert.That(added.Id, Is.EqualTo(6));
    }

    [Test]
    public async Task NextId_SkipsAnIdThatWasTakenByAnExplicitUpdate()
    {
        var repo = NewRepository(UniqueCollection("id_generator_skip"));
        var first = await repo.AddAsync(new Film { Name = "one" });
        await repo.UpdateAsync(first.Id + 1, new Film { Name = "taken by update" });

        var next = await repo.AddAsync(new Film { Name = "after the gap" });

        Assert.That(next.Id, Is.EqualTo(first.Id + 2));
    }

    [Test]
    public async Task Collections_KeepIndependentCounters()
    {
        var a = NewRepository(UniqueCollection("id_generator_a"));
        var b = NewRepository(UniqueCollection("id_generator_b"));

        await a.AddAsync(new Film { Name = "a1" });
        await a.AddAsync(new Film { Name = "a2" });
        var fromB = await b.AddAsync(new Film { Name = "b1" });

        Assert.That(fromB.Id, Is.EqualTo(1));
    }
}
