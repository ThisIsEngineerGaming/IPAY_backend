using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class GoogleLoginServiceTests
    {
        private Mock<IFirebaseAuthService> _firebase = null!;
        private Mock<IUser<Customer>> _customers = null!;
        private Mock<IJWT> _jwt = null!;
        private Mock<IRefreshTokenService> _refresh = null!;
        private GoogleLoginService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _firebase = new Mock<IFirebaseAuthService>();
            _customers = new Mock<IUser<Customer>>();
            _jwt = new Mock<IJWT>();
            _refresh = new Mock<IRefreshTokenService>();
            _refresh.Setup(r => r.CreateAuthResponseAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<UserRole>(), It.IsAny<string?>()))
                .ReturnsAsync(new AuthResponse { AccessToken = "access" });
            _sut = new GoogleLoginService(_firebase.Object, _customers.Object, _jwt.Object, _refresh.Object);
        }

        private void GivenIdentity(string email = "Alice@Test.com", string? name = "Alice", bool verified = true) =>
            _firebase.Setup(f => f.VerifyIdTokenAsync("token")).ReturnsAsync(new FirebaseIdentity("uid", email, name, verified));

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public async Task LoginWithGoogleAsync_WithBlankToken_ReturnsNullWithoutCallingFirebase(string? token)
        {
            Assert.That(await _sut.LoginWithGoogleAsync(token!), Is.Null);
            _firebase.Verify(f => f.VerifyIdTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task LoginWithGoogleAsync_WhenFirebaseRejectsTheToken_ReturnsNull()
        {
            _firebase.Setup(f => f.VerifyIdTokenAsync("token")).ReturnsAsync((FirebaseIdentity?)null);

            Assert.That(await _sut.LoginWithGoogleAsync("token"), Is.Null);
        }

        [Test]
        public async Task LoginWithGoogleAsync_WhenGoogleDoesNotVouchForTheEmail_ReturnsNull()
        {
            GivenIdentity(verified: false);

            Assert.That(await _sut.LoginWithGoogleAsync("token"), Is.Null);
            _customers.Verify(c => c.GetByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task LoginWithGoogleAsync_WithEmptyEmail_ReturnsNull()
        {
            GivenIdentity(email: " ");

            Assert.That(await _sut.LoginWithGoogleAsync("token"), Is.Null);
        }

        [Test]
        public async Task LoginWithGoogleAsync_ForNewUser_CreatesAPasswordlessVerifiedCustomer()
        {
            GivenIdentity(email: "  Alice@Test.com ", name: " Alice A ");
            var created = new Customer { Id = 9, Email = "alice@test.com", Name = "Alice A", Role = UserRole.Customer };
            _customers.SetupSequence(c => c.GetByEmail("alice@test.com"))
                .ReturnsAsync((Customer?)null)
                .ReturnsAsync(created);

            var result = await _sut.LoginWithGoogleAsync("token");

            Assert.That(result, Is.Not.Null);
            _customers.Verify(c => c.CreateAsync(It.Is<Customer>(u =>
                u.Email == "alice@test.com" &&
                u.Name == "Alice A" &&
                u.Password == string.Empty &&
                u.Role == UserRole.Customer &&
                u.EmailVerified == true)), Times.Once);
            _refresh.Verify(r => r.CreateAuthResponseAsync(9, "alice@test.com", "Alice A", UserRole.Customer, null), Times.Once);
        }

        [Test]
        public async Task LoginWithGoogleAsync_ForNewUserWithoutAName_UsesTheEmailPrefix()
        {
            GivenIdentity(email: "bob@test.com", name: null);
            _customers.SetupSequence(c => c.GetByEmail("bob@test.com"))
                .ReturnsAsync((Customer?)null)
                .ReturnsAsync(new Customer { Id = 2, Email = "bob@test.com", Name = "bob" });

            await _sut.LoginWithGoogleAsync("token");

            _customers.Verify(c => c.CreateAsync(It.Is<Customer>(u => u.Name == "bob")), Times.Once);
        }

        [Test]
        public async Task LoginWithGoogleAsync_ForExistingUnverifiedAccount_MarksItVerified()
        {
            GivenIdentity(email: "alice@test.com");
            var existing = new Customer { Id = 3, Email = "alice@test.com", Name = "Alice", EmailVerified = false };
            _customers.Setup(c => c.GetByEmail("alice@test.com")).ReturnsAsync(existing);

            var result = await _sut.LoginWithGoogleAsync("token");

            Assert.That(result, Is.Not.Null);
            Assert.That(existing.EmailVerified, Is.True);
            _customers.Verify(c => c.UpdateAsync(3, existing), Times.Once);
            _customers.Verify(c => c.CreateAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task LoginWithGoogleAsync_ForAlreadyVerifiedAccount_DoesNotRewriteIt()
        {
            GivenIdentity(email: "alice@test.com");
            _customers.Setup(c => c.GetByEmail("alice@test.com"))
                .ReturnsAsync(new Customer { Id = 3, Email = "alice@test.com", EmailVerified = true, Role = UserRole.Seller, Name = "Alice" });

            await _sut.LoginWithGoogleAsync("token");

            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
            _refresh.Verify(r => r.CreateAuthResponseAsync(3, "alice@test.com", "Alice", UserRole.Seller, null), Times.Once);
        }

        [Test]
        public async Task LoginWithGoogleAsync_ForBannedAccount_ReturnsNull()
        {
            GivenIdentity(email: "alice@test.com");
            _customers.Setup(c => c.GetByEmail("alice@test.com"))
                .ReturnsAsync(new Customer { Id = 3, Email = "alice@test.com", EmailVerified = true, IsBanned = true });

            Assert.That(await _sut.LoginWithGoogleAsync("token"), Is.Null);
            _refresh.Verify(r => r.CreateAuthResponseAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<UserRole>(), It.IsAny<string?>()), Times.Never);
        }

        [Test]
        public async Task LoginWithGoogleAsync_WhenCreatingTheAccountFails_ReturnsNull()
        {
            GivenIdentity(email: "alice@test.com");
            _customers.Setup(c => c.GetByEmail("alice@test.com")).ReturnsAsync((Customer?)null);

            Assert.That(await _sut.LoginWithGoogleAsync("token"), Is.Null);
        }
    }
}
