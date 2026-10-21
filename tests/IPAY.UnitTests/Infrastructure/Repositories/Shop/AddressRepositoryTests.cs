using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Repositories.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Shop;

[TestFixture]
public class AddressRepositoryTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAndGetById_RoundTripsAddress()
    {
        var repo = new AddressRepository(Db);
        var address = new Address { Country = "Ukraine", City = "Odesa", Street = "Test street", UserId = 81001 };
        var added = await repo.AddAsync(address);
        try
        {
            var loaded = await repo.GetByIdAsync(added.Id);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.City, Is.EqualTo("Odesa"));
            Assert.That(loaded.UserId, Is.EqualTo(81001));
        }
        finally { await repo.DeleteAsync(added.Id); }
    }

    [Test]
    public async Task GetByUserIdAndId_EnforcesOwnership()
    {
        var repo = new AddressRepository(Db);
        var added = await repo.AddAsync(new Address { Country = "Ukraine", City = "Odesa", UserId = 81002 });
        try
        {
            Assert.That(await repo.GetByUserIdAndIdAsync(81002, added.Id), Is.Not.Null);
            Assert.That(await repo.GetByUserIdAndIdAsync(99999, added.Id), Is.Null);
            Assert.That(await repo.GetByUserIdAndIdAsync(81002, int.MaxValue), Is.Null);
        }
        finally { await repo.DeleteAsync(added.Id); }
    }
}
