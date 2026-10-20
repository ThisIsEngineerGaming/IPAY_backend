using IPAY.Application.Interfaces.Auth;
using IPAY.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Caching
{
    [TestFixture]
    public class MemoryTwoFactorStoreTests
    {
        private MemoryCache _cache = null!;
        private MemoryTwoFactorStore _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _sut = new MemoryTwoFactorStore(_cache);
        }

        [TearDown]
        public void TearDown() => _cache.Dispose();

        [Test]
        public void Get_ReturnsWhatWasSaved()
        {
            var challenge = new TwoFactorChallenge { Id = "c1", UserId = 3 };
            _sut.Save(challenge, TimeSpan.FromMinutes(5));

            Assert.That(_sut.Get("c1"), Is.SameAs(challenge));
        }

        [Test]
        public void Get_ForUnknownId_ReturnsNull()
        {
            Assert.That(_sut.Get("nope"), Is.Null);
        }

        [Test]
        public void Remove_DeletesTheChallenge()
        {
            _sut.Save(new TwoFactorChallenge { Id = "c1" }, TimeSpan.FromMinutes(5));

            _sut.Remove("c1");

            Assert.That(_sut.Get("c1"), Is.Null);
        }

        [Test]
        public void Save_WithTheSameIdTwice_ReplacesTheOldChallenge()
        {
            _sut.Save(new TwoFactorChallenge { Id = "c1", FailedAttempts = 0 }, TimeSpan.FromMinutes(5));
            _sut.Save(new TwoFactorChallenge { Id = "c1", FailedAttempts = 2 }, TimeSpan.FromMinutes(5));

            Assert.That(_sut.Get("c1")!.FailedAttempts, Is.EqualTo(2));
        }

        [Test]
        public void Get_AfterTheLifetimeHasPassed_ReturnsNull()
        {
            _sut.Save(new TwoFactorChallenge { Id = "c1" }, TimeSpan.FromMilliseconds(50));

            Thread.Sleep(150);

            Assert.That(_sut.Get("c1"), Is.Null);
        }
    }
}
