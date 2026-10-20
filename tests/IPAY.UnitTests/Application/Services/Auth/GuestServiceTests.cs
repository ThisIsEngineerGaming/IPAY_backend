using AutoMapper;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Mappings.Shop;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class GuestServiceTests
    {
        private Mock<IRepository<Guest>> _repo = null!;
        private GuestService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<GuestMapping>()).CreateMapper();
            _repo = new Mock<IRepository<Guest>>();
            _repo.Setup(r => r.AddAsync(It.IsAny<Guest>())).ReturnsAsync((Guest g) => { g.Id = 10; return g; });
            _sut = new GuestService(_repo.Object, mapper);
        }

        [Test]
        public async Task GetAllAsync_MapsEveryGuest()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Guest>
            {
                new() { Id = 1, Name = "A", SessionKey = "k1" },
                new() { Id = 2, Name = "B", SessionKey = "k2" }
            });

            var result = await _sut.GetAllAsync();

            Assert.That(result.Select(g => g.Id), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(result.Select(g => g.SessionKey), Is.EqualTo(new[] { "k1", "k2" }));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Guest { Id = 1, Name = "A", IsBanned = true });

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result!.Name, Is.EqualTo("A"));
            Assert.That(result.IsBanned, Is.True);
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Guest?)null);

            Assert.That(await _sut.GetByIdAsync(1), Is.Null);
        }

        [Test]
        public async Task GetOrCreateAsync_WithKnownSessionKey_ReturnsTheExistingGuestWithoutCreatingOne()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Guest>
            {
                new() { Id = 3, Name = "Old", SessionKey = "abc" }
            });

            var result = await _sut.GetOrCreateAsync("abc");

            Assert.That(result.Id, Is.EqualTo(3));
            _repo.Verify(r => r.AddAsync(It.IsAny<Guest>()), Times.Never);
        }

        [Test]
        public async Task GetOrCreateAsync_SessionKeyComparisonIsCaseSensitive()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Guest>
            {
                new() { Id = 3, SessionKey = "ABC" }
            });

            await _sut.GetOrCreateAsync("abc");

            _repo.Verify(r => r.AddAsync(It.IsAny<Guest>()), Times.Once);
        }

        [Test]
        public async Task GetOrCreateAsync_WithNewKeyAndName_CreatesAGuestWithThem()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Guest>());
            Guest? saved = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<Guest>()))
                .Callback<Guest>(g => saved = g)
                .ReturnsAsync((Guest g) => { g.Id = 10; return g; });

            var result = await _sut.GetOrCreateAsync("my-key", "  Bob  ");

            Assert.That(saved!.SessionKey, Is.EqualTo("my-key"));
            Assert.That(saved.Name, Is.EqualTo("Bob"));
            Assert.That(saved.Role, Is.EqualTo(UserRole.Guest));
            Assert.That(saved.IsBanned, Is.False);
            Assert.That(result.Id, Is.EqualTo(10));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task GetOrCreateAsync_WithoutKeyOrName_GeneratesKeyAndUsesDefaultName(string? blank)
        {
            Guest? saved = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<Guest>()))
                .Callback<Guest>(g => saved = g)
                .ReturnsAsync((Guest g) => { g.Id = 10; return g; });

            await _sut.GetOrCreateAsync(blank, blank);

            Assert.That(saved!.Name, Is.EqualTo("Guest"));
            Assert.That(saved.SessionKey, Has.Length.EqualTo(32));
            _repo.Verify(r => r.GetAllAsync(), Times.Never);
        }

        [Test]
        public async Task CreateAsync_MapsTheSessionDtoAndReturnsTheCreatedGuest()
        {
            var result = await _sut.CreateAsync(new GuestSessionDto { SessionKey = "k", Name = " Zed " });

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Zed"));
            Assert.That(result.SessionKey, Is.EqualTo("k"));
            Assert.That(result.Role, Is.EqualTo(UserRole.Guest));
        }

        [Test]
        public async Task CreateAsync_WhenRepositoryReturnsNothing_ReturnsNull()
        {
            _repo.Setup(r => r.AddAsync(It.IsAny<Guest>())).ReturnsAsync((Guest)null!);

            Assert.That(await _sut.CreateAsync(new GuestSessionDto()), Is.Null);
        }

        [Test]
        public async Task UpdateAsync_WhenMissing_ReturnsNullAndSavesNothing()
        {
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Guest?)null);

            var result = await _sut.UpdateAsync(1, new UpdateGuestDto { Name = "x" });

            Assert.That(result, Is.Null);
            _repo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Guest>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_AppliesOnlyTheFieldsThatWereSent()
        {
            var guest = new Guest { Id = 1, Name = "Old", IsBanned = false, SessionKey = "keep" };
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(guest);

            var result = await _sut.UpdateAsync(1, new UpdateGuestDto { IsBanned = true });

            Assert.That(result!.IsBanned, Is.True);
            Assert.That(result.Name, Is.EqualTo("Old"));
            Assert.That(guest.SessionKey, Is.EqualTo("keep"));
            _repo.Verify(r => r.UpdateAsync(1, guest), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_CanRenameTheGuest()
        {
            var guest = new Guest { Id = 1, Name = "Old" };
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(guest);

            var result = await _sut.UpdateAsync(1, new UpdateGuestDto { Name = "New" });

            Assert.That(result!.Name, Is.EqualTo("New"));
        }

        [Test]
        public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
        {
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Guest { Id = 1 });

            Assert.That(await _sut.DeleteAsync(1), Is.True);
            _repo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WhenMissing_ReturnsFalse()
        {
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Guest?)null);

            Assert.That(await _sut.DeleteAsync(1), Is.False);
            _repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
