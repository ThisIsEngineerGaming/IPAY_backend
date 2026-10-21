using IPAY.UnitTests.Infrastructure;
using IPAY.Domain.Entities.Auth;
using IPAY.Infrastructure.Repositories.Auth;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories.Auth;

[TestFixture]
public class RefreshTokenRepositoryTests : FirestoreEmulatorTestBase
{
    [Test]
    public async Task AddAndGetById_RoundTripsToken()
    {
        var repo = new RefreshTokenRepository(Db);
        var id = "test_" + Guid.NewGuid().ToString("N");
        var token = new RefreshToken { Id = id, UserId = 41, Email = "person@example.com", Role = "User", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        try
        {
            await repo.AddAsync(token);
            var loaded = await repo.GetByIdAsync(id);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.UserId, Is.EqualTo(41));
            Assert.That(loaded.Email, Is.EqualTo("person@example.com"));
        }
        finally { await Db.Collection("refreshTokens").Document(id).DeleteAsync(); }
    }

    [Test]
    public async Task GetById_ReturnsNullWhenTokenDoesNotExist()
    {
        var repo = new RefreshTokenRepository(Db);
        Assert.That(await repo.GetByIdAsync("missing_" + Guid.NewGuid().ToString("N")), Is.Null);
    }

    [Test]
    public async Task RevokeAllForUser_RevokesOnlyActiveTokensForThatUser()
    {
        var repo = new RefreshTokenRepository(Db);
        var prefix = Guid.NewGuid().ToString("N");
        var first = new RefreshToken { Id = prefix + "_1", UserId = 8321, Email = "a@b.com", Role = "User", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        var second = new RefreshToken { Id = prefix + "_2", UserId = 8321, Email = "a@b.com", Role = "User", ExpiresAt = DateTime.UtcNow.AddDays(1), IsRevoked = true };
        var other = new RefreshToken { Id = prefix + "_3", UserId = 8322, Email = "c@d.com", Role = "User", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        try
        {
            await repo.AddAsync(first); await repo.AddAsync(second); await repo.AddAsync(other);
            await repo.RevokeAllForUserAsync(8321);
            Assert.That((await repo.GetByIdAsync(first.Id))!.IsRevoked, Is.True);
            Assert.That((await repo.GetByIdAsync(second.Id))!.IsRevoked, Is.True);
            Assert.That((await repo.GetByIdAsync(other.Id))!.IsRevoked, Is.False);
        }
        finally
        {
            await Db.Collection("refreshTokens").Document(first.Id).DeleteAsync();
            await Db.Collection("refreshTokens").Document(second.Id).DeleteAsync();
            await Db.Collection("refreshTokens").Document(other.Id).DeleteAsync();
        }
    }
}
