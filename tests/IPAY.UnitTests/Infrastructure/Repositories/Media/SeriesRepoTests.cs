using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Repositories.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Media;

[TestFixture]
public class SeriesRepoTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task GetPageAsync_FiltersByGenreAndPrefix()
    {
        var repo = new SeriesRepo(Db, UniqueCollection("series_page"));
        var first = await repo.AddAsync(new Series { Name = "Northern Lights", GenreIds = new() { 6 } });
        var second = await repo.AddAsync(new Series { Name = "North Shore", GenreIds = new() { 6 } });
        var other = await repo.AddAsync(new Series { Name = "Southern Road", GenreIds = new() { 6 } });
        try
        {
            var result = await repo.GetPageAsync(10, null, 6, "north", null, null);
            Assert.That(result.Select(x => x.Name), Does.Contain("Northern Lights"));
            Assert.That(result.Select(x => x.Name), Does.Contain("North Shore"));
            Assert.That(result.Select(x => x.Name), Does.Not.Contain("Southern Road"));
        }
        finally { await repo.DeleteAsync(first.Id); await repo.DeleteAsync(second.Id); await repo.DeleteAsync(other.Id); }
    }
}
