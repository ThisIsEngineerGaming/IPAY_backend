using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Application.DTOs.Auth;
using IPAY.Domain.Entities.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos.Auth;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class RefreshTokenServiceTests
    {
        private Mock<IJWT> _jwt = null!;
        private Mock<IRefreshTokenRepository> _repo = null!;
        private Mock<IUser<Customer>> _customers = null!;
        private Mock<IGuestService> _guests = null!;
        private RefreshTokenService _sut = null!;

        private static IConfiguration Config(params (string Key, string Value)[] values) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value))
                .Build();

        private RefreshTokenService CreateSut(IConfiguration? config = null) =>
            new(_jwt.Object, _repo.Object, _customers.Object, _guests.Object, config ?? Config());

        [SetUp]
        public void SetUp()
        {
            _jwt = new Mock<IJWT>();
            _jwt.Setup(j => j.GenerateRefreshToken()).Returns("raw-new");
            _jwt.Setup(j => j.HashToken(It.IsAny<string>())).Returns((string raw) => "hash:" + raw);
            _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>()))
                .Returns("access-token");

            _repo = new Mock<IRefreshTokenRepository>();
            _customers = new Mock<IUser<Customer>>();
            _guests = new Mock<IGuestService>();

            _sut = CreateSut();
        }

        private static RefreshToken ActiveToken(int userId = 7, UserRole role = UserRole.Customer, string email = "a@test.com") => new()
        {
            Id = "hash:raw-old",
            UserId = userId,
            Role = role.ToString(),
            Email = email,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // ---------- IssuePairAsync ----------

        [Test]
        public async Task IssuePairAsync_StoresOnlyTheHashOfTheTokenAndReturnsTheRawOne()
        {
            RefreshToken? saved = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Callback<RefreshToken>(t => saved = t).Returns(Task.CompletedTask);

            var (raw, _) = await _sut.IssuePairAsync(7, "a@test.com", UserRole.Seller, "1.2.3.4");

            Assert.That(raw, Is.EqualTo("raw-new"));
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Id, Is.EqualTo("hash:raw-new"));
            Assert.That(saved.UserId, Is.EqualTo(7));
            Assert.That(saved.Email, Is.EqualTo("a@test.com"));
            Assert.That(saved.Role, Is.EqualTo("Seller"));
            Assert.That(saved.CreatedByIp, Is.EqualTo("1.2.3.4"));
        }

        [Test]
        public async Task IssuePairAsync_WithoutConfig_UsesDefaultLifetimes()
        {
            RefreshToken? saved = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Callback<RefreshToken>(t => saved = t).Returns(Task.CompletedTask);
            var before = DateTime.UtcNow;

            var (_, accessExpires) = await _sut.IssuePairAsync(1, "a@test.com", UserRole.Customer);

            var after = DateTime.UtcNow;
            Assert.That(saved!.ExpiresAt, Is.InRange(before.AddDays(14), after.AddDays(14)));
            Assert.That(accessExpires, Is.InRange(before.AddMinutes(20), after.AddMinutes(20)));
        }

        [Test]
        public async Task IssuePairAsync_UsesConfiguredLifetimes()
        {
            var sut = CreateSut(Config(("Jwt:AccessTokenMinutes", "5"), ("Jwt:RefreshTokenDays", "2")));
            RefreshToken? saved = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Callback<RefreshToken>(t => saved = t).Returns(Task.CompletedTask);
            var before = DateTime.UtcNow;

            var (_, accessExpires) = await sut.IssuePairAsync(1, "a@test.com", UserRole.Customer);

            var after = DateTime.UtcNow;
            Assert.That(saved!.ExpiresAt, Is.InRange(before.AddDays(2), after.AddDays(2)));
            Assert.That(accessExpires, Is.InRange(before.AddMinutes(5), after.AddMinutes(5)));
        }

        [Test]
        public async Task IssuePairAsync_WithGarbageConfig_FallsBackToDefaults()
        {
            var sut = CreateSut(Config(("Jwt:AccessTokenMinutes", "abc"), ("Jwt:RefreshTokenDays", "xyz")));
            RefreshToken? saved = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Callback<RefreshToken>(t => saved = t).Returns(Task.CompletedTask);
            var before = DateTime.UtcNow;

            await sut.IssuePairAsync(1, "a@test.com", UserRole.Customer);

            Assert.That(saved!.ExpiresAt, Is.InRange(before.AddDays(14), DateTime.UtcNow.AddDays(14)));
        }

        // ---------- RefreshAsync: rejected tokens ----------

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task RefreshAsync_WithBlankToken_ReturnsNullWithoutTouchingTheRepo(string? raw)
        {
            var result = await _sut.RefreshAsync(raw!);

            Assert.That(result, Is.Null);
            _repo.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task RefreshAsync_WithUnknownToken_ReturnsNull()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync((RefreshToken?)null);

            var result = await _sut.RefreshAsync("raw-old");

            Assert.That(result, Is.Null);
            _repo.Verify(r => r.RevokeAllForUserAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task RefreshAsync_WithAlreadyRevokedToken_RevokesEveryTokenOfThatUser()
        {
            var token = ActiveToken(userId: 7);
            token.IsRevoked = true;
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(token);

            var result = await _sut.RefreshAsync("raw-old");

            Assert.That(result, Is.Null);
            _repo.Verify(r => r.RevokeAllForUserAsync(7), Times.Once);
        }

        [Test]
        public async Task RefreshAsync_WithExpiredToken_ReturnsNull()
        {
            var token = ActiveToken();
            token.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(token);

            var result = await _sut.RefreshAsync("raw-old");

            Assert.That(result, Is.Null);
            _repo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Test]
        public async Task RefreshAsync_WithUnknownRoleString_ReturnsNull()
        {
            var token = ActiveToken();
            token.Role = "SuperUser";
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(token);

            var result = await _sut.RefreshAsync("raw-old");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RefreshAsync_WhenCustomerNoLongerExists_ReturnsNull()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(ActiveToken());
            _customers.Setup(c => c.GetByIdAsync(7)).ReturnsAsync((Customer?)null);

            Assert.That(await _sut.RefreshAsync("raw-old"), Is.Null);
        }

        [Test]
        public async Task RefreshAsync_WhenCustomerIsBanned_ReturnsNull()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(ActiveToken());
            _customers.Setup(c => c.GetByIdAsync(7)).ReturnsAsync(new Customer { Id = 7, IsBanned = true });

            Assert.That(await _sut.RefreshAsync("raw-old"), Is.Null);
        }

        [Test]
        public async Task RefreshAsync_WhenCustomerHasNoId_ReturnsNull()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(ActiveToken());
            _customers.Setup(c => c.GetByIdAsync(7)).ReturnsAsync(new Customer { Id = null });

            Assert.That(await _sut.RefreshAsync("raw-old"), Is.Null);
        }

        [Test]
        public async Task RefreshAsync_WhenGuestMissingOrBanned_ReturnsNull()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(ActiveToken(role: UserRole.Guest));

            _guests.Setup(g => g.GetByIdAsync(7)).ReturnsAsync((GuestDto?)null);
            Assert.That(await _sut.RefreshAsync("raw-old"), Is.Null);

            _guests.Setup(g => g.GetByIdAsync(7)).ReturnsAsync(new GuestDto { Id = 7, IsBanned = true });
            Assert.That(await _sut.RefreshAsync("raw-old"), Is.Null);
        }

        // ---------- RefreshAsync: success ----------

        [Test]
        public async Task RefreshAsync_ForCustomer_RotatesTheTokenAndReturnsAFreshPair()
        {
            var existing = ActiveToken();
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(existing);
            _customers.Setup(c => c.GetByIdAsync(7))
                .ReturnsAsync(new Customer { Id = 7, Email = "new@test.com", Name = "Neo", Role = UserRole.Customer });

            var result = await _sut.RefreshAsync("raw-old", "9.9.9.9");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AccessToken, Is.EqualTo("access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("raw-new"));
            Assert.That(result.TokenType, Is.EqualTo("Bearer"));
            Assert.That(result.User.Email, Is.EqualTo("new@test.com"));
            Assert.That(result.User.Name, Is.EqualTo("Neo"));

            Assert.That(existing.IsRevoked, Is.True);
            Assert.That(existing.ReplacedByTokenId, Is.EqualTo("hash:raw-new"));
            _repo.Verify(r => r.UpdateAsync(existing), Times.Once);
            _repo.Verify(r => r.AddAsync(It.Is<RefreshToken>(t => t.CreatedByIp == "9.9.9.9" && t.UserId == 7)), Times.Once);
        }

        [Test]
        public async Task RefreshAsync_UsesTheUsersCurrentRoleNotTheOneStoredInTheToken()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(ActiveToken(role: UserRole.Customer));
            _customers.Setup(c => c.GetByIdAsync(7))
                .ReturnsAsync(new Customer { Id = 7, Email = "a@test.com", Role = UserRole.Seller });

            await _sut.RefreshAsync("raw-old");

            _jwt.Verify(j => j.GenerateAccessToken("7", "a@test.com", UserRole.Seller), Times.Once);
            _repo.Verify(r => r.AddAsync(It.Is<RefreshToken>(t => t.Role == "Seller")), Times.Once);
        }

        [Test]
        public async Task RefreshAsync_WhenCustomerEmailIsNull_FallsBackToTheEmailStoredOnTheToken()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(ActiveToken(email: "stored@test.com"));
            _customers.Setup(c => c.GetByIdAsync(7)).ReturnsAsync(new Customer { Id = 7, Email = null });

            var result = await _sut.RefreshAsync("raw-old");

            Assert.That(result!.User.Email, Is.EqualTo("stored@test.com"));
        }

        [Test]
        public async Task RefreshAsync_ForGuest_UsesASyntheticGuestEmail()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(ActiveToken(userId: 12, role: UserRole.Guest, email: "ignored"));
            _guests.Setup(g => g.GetByIdAsync(12)).ReturnsAsync(new GuestDto { Id = 12, Name = "Visitor" });

            var result = await _sut.RefreshAsync("raw-old");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.User.Email, Is.EqualTo("guest-12@guest.local"));
            Assert.That(result.User.Name, Is.EqualTo("Visitor"));
            _jwt.Verify(j => j.GenerateAccessToken("12", "guest-12@guest.local", UserRole.Guest), Times.Once);
            _customers.Verify(c => c.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        // ---------- RevokeAsync / RevokeAllForUserAsync ----------

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task RevokeAsync_WithBlankToken_DoesNothing(string? raw)
        {
            await _sut.RevokeAsync(raw!);

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
            _repo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Test]
        public async Task RevokeAsync_WithUnknownToken_DoesNothing()
        {
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync((RefreshToken?)null);

            await _sut.RevokeAsync("raw-old");

            _repo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Test]
        public async Task RevokeAsync_WithAlreadyRevokedToken_DoesNotWriteAgain()
        {
            var token = ActiveToken();
            token.IsRevoked = true;
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(token);

            await _sut.RevokeAsync("raw-old");

            _repo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Test]
        public async Task RevokeAsync_WithActiveToken_MarksItRevoked()
        {
            var token = ActiveToken();
            _repo.Setup(r => r.GetByIdAsync("hash:raw-old")).ReturnsAsync(token);

            await _sut.RevokeAsync("raw-old");

            Assert.That(token.IsRevoked, Is.True);
            _repo.Verify(r => r.UpdateAsync(token), Times.Once);
        }

        [Test]
        public async Task RevokeAllForUserAsync_DelegatesToRepository()
        {
            await _sut.RevokeAllForUserAsync(7);

            _repo.Verify(r => r.RevokeAllForUserAsync(7), Times.Once);
        }

        // ---------- CreateAuthResponseAsync ----------

        [Test]
        public async Task CreateAuthResponseAsync_BuildsAResponseWithBothTokens()
        {
            var result = await _sut.CreateAuthResponseAsync(7, "a@test.com", "Alice", UserRole.Customer, "1.1.1.1");

            Assert.That(result.AccessToken, Is.EqualTo("access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("raw-new"));
            Assert.That(result.TokenType, Is.EqualTo("Bearer"));
            Assert.That(result.User.Email, Is.EqualTo("a@test.com"));
            Assert.That(result.User.Name, Is.EqualTo("Alice"));
            Assert.That(result.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));
            _repo.Verify(r => r.AddAsync(It.Is<RefreshToken>(t => t.CreatedByIp == "1.1.1.1")), Times.Once);
        }
    }
}
