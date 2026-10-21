using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Repositories.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Media;

[TestFixture]
public class EpisodeRepoTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task GetBySeriesId_ReturnsEpisodesInNumericIdOrder()
    {
        var collection = UniqueCollection("episodes");
        var repo = new EpisodeRepo(Db, collection);
        var one = await repo.AddAsync(new Episode { Name = "one", SerialId = 9 });
        var two = await repo.AddAsync(new Episode { Name = "two", SerialId = 9 });
        try
        {
            var result = await repo.GetBySeriesIdAsync(9);
            Assert.That(result.Select(x => x.Id), Is.Ordered.Ascending);
            Assert.That(result.Select(x => x.Name), Does.Contain("one"));
            Assert.That(result.Select(x => x.Name), Does.Contain("two"));
            Assert.That(await repo.GetBySeriesIdAsync(123456), Is.Empty);
        }
        finally { await repo.DeleteAsync(one.Id); await repo.DeleteAsync(two.Id); }
    }

    [Test]
    public async Task GetById_ReturnsNullForMissingEpisode()
    {
        var repo = new EpisodeRepo(Db, UniqueCollection("episode_missing"));
        Assert.That(await repo.GetByIdAsync(456789), Is.Null);
    }
}
