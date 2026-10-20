using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Application.Validators.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class ProfileServiceTests
    {
        private const string OldPassword = "OldPass1!";
        private const string NewPassword = "NewPass1!";

        private Mock<IUser<Customer>> _customers = null!;
        private Mock<IPasswordHashingService> _hasher = null!;
        private Mock<IFirebaseAuthService> _firebase = null!;
        private Mock<IJWT> _jwt = null!;
        private Mock<IRefreshTokenService> _refresh = null!;
        private ProfileService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _customers = new Mock<IUser<Customer>>();
            // Fake hashing keeps the tests fast (the real one runs 210k PBKDF2 rounds).
            _hasher = new Mock<IPasswordHashingService>();
            _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns((string p) => "hash:" + p);
            _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns((string p, string h) => h == "hash:" + p);

            _firebase = new Mock<IFirebaseAuthService>();
            _jwt = new Mock<IJWT>();
            _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>())).Returns("jwt");
            _refresh = new Mock<IRefreshTokenService>();
            _refresh.Setup(r => r.CreateAuthResponseAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<UserRole>(), It.IsAny<string?>()))
                .ReturnsAsync(new AuthResponse { AccessToken = "fresh" });

            // Real validators: they are small, and this way a rule change shows up here too.
            _sut = new ProfileService(
                _customers.Object, _hasher.Object, _firebase.Object, _jwt.Object,
                new ChangeUsernameValidator(), new ChangePhoneValidator(), new ChangePasswordValidator(),
                new ResetForgottenPasswordValidator(), new StartEmailChangeValidator(), new ConfirmEmailChangeValidator(),
                _refresh.Object);
        }

        private Customer GivenUser(int id = 1, string? password = OldPassword, bool banned = false, string email = "alice@test.com")
        {
            var user = new Customer
            {
                Id = id,
                Name = "Alice",
                Email = email,
                Phone = null,
                Password = password is null ? string.Empty : "hash:" + password,
                IsBanned = banned,
                Role = UserRole.Customer
            };
            _customers.Setup(c => c.GetByIdAsync(id)).ReturnsAsync(user);
            return user;
        }

        // ---------- GetProfileAsync ----------

        [Test]
        public async Task GetProfileAsync_ReturnsTheProfileOfTheAccount()
        {
            var user = GivenUser();
            user.Phone = "+380123456789";

            var result = await _sut.GetProfileAsync(1);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Value!.Name, Is.EqualTo("Alice"));
            Assert.That(result.Value.Email, Is.EqualTo("alice@test.com"));
            Assert.That(result.Value.Phone, Is.EqualTo("+380123456789"));
            Assert.That(result.Value.HasPassword, Is.True);
        }

        [Test]
        public async Task GetProfileAsync_ForAGoogleAccount_ReportsNoPassword()
        {
            GivenUser(password: null);

            var result = await _sut.GetProfileAsync(1);

            Assert.That(result.Value!.HasPassword, Is.False);
            Assert.That(result.Value.Phone, Is.Empty);
        }

        [Test]
        public async Task GetProfileAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            var result = await _sut.GetProfileAsync(1);

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task GetProfileAsync_ForABannedAccount_IsNotFound()
        {
            GivenUser(banned: true);

            var result = await _sut.GetProfileAsync(1);

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        // ---------- ChangeUsernameAsync ----------

        [Test]
        public async Task ChangeUsernameAsync_WithInvalidName_ReturnsTheValidationMessage()
        {
            GivenUser();

            var result = await _sut.ChangeUsernameAsync(1, new ChangeUsernameDto { NewName = "A1", Password = OldPassword });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
            Assert.That(result.Error, Is.Not.Empty);
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task ChangeUsernameAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            var result = await _sut.ChangeUsernameAsync(1, new ChangeUsernameDto { NewName = "Bobby", Password = OldPassword });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task ChangeUsernameAsync_WithoutPassword_AsksForIt()
        {
            GivenUser();

            var result = await _sut.ChangeUsernameAsync(1, new ChangeUsernameDto { NewName = "Bobby", Password = "" });

            Assert.That(result.Error, Does.Contain("Enter your password"));
        }

        [Test]
        public async Task ChangeUsernameAsync_WithWrongPassword_IsRejected()
        {
            GivenUser();

            var result = await _sut.ChangeUsernameAsync(1, new ChangeUsernameDto { NewName = "Bobby", Password = "Wrong123!" });

            Assert.That(result.Error, Is.EqualTo("Incorrect password."));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task ChangeUsernameAsync_WithRightPassword_SavesTheTrimmedName()
        {
            var user = GivenUser();

            var result = await _sut.ChangeUsernameAsync(1, new ChangeUsernameDto { NewName = "  Bobby  ", Password = OldPassword });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Value!.Name, Is.EqualTo("Bobby"));
            _customers.Verify(c => c.UpdateAsync(1, user), Times.Once);
        }

        [Test]
        public async Task ChangeUsernameAsync_ForAGoogleAccount_NeedsNoPassword()
        {
            GivenUser(password: null);

            var result = await _sut.ChangeUsernameAsync(1, new ChangeUsernameDto { NewName = "Bobby", Password = "" });

            Assert.That(result.Succeeded, Is.True);
        }

        // ---------- ChangePhoneAsync ----------

        [Test]
        public async Task ChangePhoneAsync_WithInvalidNumber_IsRejected()
        {
            GivenUser();

            var result = await _sut.ChangePhoneAsync(1, new ChangePhoneDto { NewPhone = "abc", Password = OldPassword });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
        }

        [Test]
        public async Task ChangePhoneAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            var result = await _sut.ChangePhoneAsync(1, new ChangePhoneDto { NewPhone = "+380123456789", Password = OldPassword });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task ChangePhoneAsync_WithWrongPassword_IsRejected()
        {
            GivenUser();

            var result = await _sut.ChangePhoneAsync(1, new ChangePhoneDto { NewPhone = "+380123456789", Password = "nope" });

            Assert.That(result.Error, Is.EqualTo("Incorrect password."));
        }

        [Test]
        public async Task ChangePhoneAsync_StoresTheNormalisedNumber()
        {
            var user = GivenUser();

            var result = await _sut.ChangePhoneAsync(1, new ChangePhoneDto { NewPhone = "+380 (12) 345-6789", Password = OldPassword });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(user.Phone, Is.EqualTo("+380123456789"));
            Assert.That(result.Value!.Phone, Is.EqualTo("+380123456789"));
            _customers.Verify(c => c.UpdateAsync(1, user), Times.Once);
        }

        // ---------- ChangePasswordAsync ----------

        [Test]
        public async Task ChangePasswordAsync_WithWeakNewPassword_IsRejected()
        {
            GivenUser();

            var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = OldPassword, NewPassword = "weak" });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
            _firebase.Verify(f => f.SetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ChangePasswordAsync_WithSamePassword_IsRejected()
        {
            GivenUser();

            var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = OldPassword, NewPassword = OldPassword });

            Assert.That(result.Error, Does.Contain("different"));
        }

        [Test]
        public async Task ChangePasswordAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = OldPassword, NewPassword = NewPassword });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task ChangePasswordAsync_ForAccountWithoutPassword_PointsToCreatePassword()
        {
            GivenUser(password: null);

            var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = OldPassword, NewPassword = NewPassword });

            Assert.That(result.Error, Does.Contain("no password"));
        }

        [Test]
        public async Task ChangePasswordAsync_WithWrongCurrentPassword_IsRejected()
        {
            GivenUser();

            var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = "Other123!", NewPassword = NewPassword });

            Assert.That(result.Error, Does.Contain("Current password is incorrect"));
            _firebase.Verify(f => f.SetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ChangePasswordAsync_UpdatesFirebaseAndOurCopy()
        {
            var user = GivenUser();

            var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = OldPassword, NewPassword = NewPassword });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(user.Password, Is.EqualTo("hash:" + NewPassword));
            _firebase.Verify(f => f.SetPasswordAsync("alice@test.com", NewPassword), Times.Once);
            _customers.Verify(c => c.UpdateAsync(1, user), Times.Once);
        }

        [Test]
        public async Task ChangePasswordAsync_WhenFirebaseFails_ChangesNothing()
        {
            var user = GivenUser();
            _firebase.Setup(f => f.SetPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("down"));

            var result = await _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = OldPassword, NewPassword = NewPassword });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
            Assert.That(user.Password, Is.EqualTo("hash:" + OldPassword));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public void ChangePasswordAsync_WhenSavingFails_PutsTheOldPasswordBackInFirebaseAndRethrows()
        {
            GivenUser();
            _customers.Setup(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>())).ThrowsAsync(new InvalidOperationException("db"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.ChangePasswordAsync(1, new ChangePasswordDto { CurrentPassword = OldPassword, NewPassword = NewPassword }));

            _firebase.Verify(f => f.SetPasswordAsync("alice@test.com", NewPassword), Times.Once);
            _firebase.Verify(f => f.SetPasswordAsync("alice@test.com", OldPassword), Times.Once);
        }

        // ---------- StartPasswordChangeAsync ----------

        [Test]
        public async Task StartPasswordChangeAsync_EnsuresAFirebaseAccountExists()
        {
            GivenUser();

            var result = await _sut.StartPasswordChangeAsync(1);

            Assert.That(result.Succeeded, Is.True);
            _firebase.Verify(f => f.EnsureAccountAsync("alice@test.com"), Times.Once);
        }

        [Test]
        public async Task StartPasswordChangeAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            Assert.That((await _sut.StartPasswordChangeAsync(1)).Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task StartPasswordChangeAsync_WithoutEmail_IsRejected()
        {
            GivenUser(email: "");

            var result = await _sut.StartPasswordChangeAsync(1);

            Assert.That(result.Error, Does.Contain("no email"));
        }

        [Test]
        public async Task StartPasswordChangeAsync_WhenFirebaseFails_ReturnsAFriendlyError()
        {
            GivenUser();
            _firebase.Setup(f => f.EnsureAccountAsync(It.IsAny<string>())).ThrowsAsync(new Exception("down"));

            var result = await _sut.StartPasswordChangeAsync(1);

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
        }

        // ---------- ConfirmPasswordChangeAsync ----------

        private void GivenValidResetProof(string email = "alice@test.com", bool verified = true,
            TimeSpan? signedInAgo = null, TimeSpan? changedAgo = null, bool hasChange = true)
        {
            var now = DateTimeOffset.UtcNow;
            _firebase.Setup(f => f.VerifyIdTokenAsync("id-token"))
                .ReturnsAsync(new FirebaseIdentity("uid", email, "Alice", verified, now - (signedInAgo ?? TimeSpan.FromMinutes(1))));
            _firebase.Setup(f => f.GetPasswordChangedAtAsync(It.IsAny<string>()))
                .ReturnsAsync(hasChange ? now - (changedAgo ?? TimeSpan.FromMinutes(2)) : (DateTimeOffset?)null);
        }

        private static ConfirmPasswordChangeDto ConfirmDto(string newPassword = NewPassword) =>
            new() { IdToken = "id-token", NewPassword = newPassword };

        [Test]
        public async Task ConfirmPasswordChangeAsync_WithWeakPassword_IsRejected()
        {
            GivenUser();

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto("weak"));

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            Assert.That((await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto())).Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WhenTokenIsNotValid_IsRejected()
        {
            GivenUser();
            _firebase.Setup(f => f.VerifyIdTokenAsync("id-token")).ReturnsAsync((FirebaseIdentity?)null);

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("could not confirm"));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WhenTokenBelongsToAnotherAccount_IsRejected()
        {
            GivenUser();
            GivenValidResetProof(email: "someone-else@test.com");

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("could not confirm"));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_EmailComparisonIgnoresCaseAndSpaces()
        {
            GivenUser();
            GivenValidResetProof(email: " ALICE@test.com ");

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WhenFirebaseEmailIsUnverified_IsRejected()
        {
            GivenUser();
            GivenValidResetProof(verified: false);

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("could not confirm"));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WithOldSignIn_SaysItExpired()
        {
            GivenUser();
            GivenValidResetProof(signedInAgo: TimeSpan.FromMinutes(11), changedAgo: TimeSpan.FromMinutes(12));

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("expired"));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WithoutAuthTime_SaysItExpired()
        {
            GivenUser();
            _firebase.Setup(f => f.VerifyIdTokenAsync("id-token"))
                .ReturnsAsync(new FirebaseIdentity("uid", "alice@test.com", null, true, null));

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("expired"));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WhenFirebasePasswordWasNeverChanged_IsRejected()
        {
            GivenUser();
            GivenValidResetProof(hasChange: false);

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("could not confirm"));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WhenPasswordChangeIsTooOld_IsRejected()
        {
            GivenUser();
            GivenValidResetProof(signedInAgo: TimeSpan.FromMinutes(1), changedAgo: TimeSpan.FromMinutes(31));

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("could not confirm"));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WhenSignInHappenedBeforeThePasswordChange_IsRejected()
        {
            // Signing in with the OLD password and skipping the emailed link must not work.
            GivenUser();
            GivenValidResetProof(signedInAgo: TimeSpan.FromMinutes(5), changedAgo: TimeSpan.FromMinutes(1));

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Error, Does.Contain("could not confirm"));
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_WithValidProof_StoresTheNewPasswordHash()
        {
            var user = GivenUser();
            GivenValidResetProof();

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Succeeded, Is.True);
            Assert.That(user.Password, Is.EqualTo("hash:" + NewPassword));
            _customers.Verify(c => c.UpdateAsync(1, user), Times.Once);
        }

        [Test]
        public async Task ConfirmPasswordChangeAsync_ForAGoogleOnlyAccount_CanCreateAPassword()
        {
            var user = GivenUser(password: null);
            GivenValidResetProof();

            var result = await _sut.ConfirmPasswordChangeAsync(1, ConfirmDto());

            Assert.That(result.Succeeded, Is.True);
            Assert.That(user.Password, Is.EqualTo("hash:" + NewPassword));
        }

        // ---------- StartForgotPasswordAsync ----------

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public async Task StartForgotPasswordAsync_WithBlankEmail_DoesNothing(string? email)
        {
            await _sut.StartForgotPasswordAsync(email!);

            _customers.Verify(c => c.GetByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task StartForgotPasswordAsync_ForKnownAccount_EnsuresAFirebaseAccount()
        {
            _customers.Setup(c => c.GetByEmail("alice@test.com"))
                .ReturnsAsync(new Customer { Id = 1, Email = "alice@test.com", Password = "hash:x" });

            await _sut.StartForgotPasswordAsync("  alice@test.com ");

            _firebase.Verify(f => f.EnsureAccountAsync("alice@test.com"), Times.Once);
        }

        [Test]
        public async Task StartForgotPasswordAsync_SilentlySkipsUnknownBannedAndGoogleOnlyAccounts()
        {
            _customers.Setup(c => c.GetByEmail("none@test.com")).ReturnsAsync((Customer?)null);
            _customers.Setup(c => c.GetByEmail("banned@test.com"))
                .ReturnsAsync(new Customer { Id = 2, Email = "banned@test.com", Password = "hash:x", IsBanned = true });
            _customers.Setup(c => c.GetByEmail("google@test.com"))
                .ReturnsAsync(new Customer { Id = 3, Email = "google@test.com", Password = "" });

            await _sut.StartForgotPasswordAsync("none@test.com");
            await _sut.StartForgotPasswordAsync("banned@test.com");
            await _sut.StartForgotPasswordAsync("google@test.com");

            _firebase.Verify(f => f.EnsureAccountAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void StartForgotPasswordAsync_WhenSomethingFails_DoesNotLeakIt()
        {
            _customers.Setup(c => c.GetByEmail(It.IsAny<string>())).ThrowsAsync(new Exception("db down"));

            Assert.DoesNotThrowAsync(() => _sut.StartForgotPasswordAsync("alice@test.com"));
        }

        // ---------- ResetForgottenPasswordAsync ----------

        [Test]
        public async Task ResetForgottenPasswordAsync_WithWeakPassword_IsRejected()
        {
            var result = await _sut.ResetForgottenPasswordAsync(ConfirmDto("weak"));

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task ResetForgottenPasswordAsync_WithInvalidToken_IsRejected()
        {
            _firebase.Setup(f => f.VerifyIdTokenAsync("id-token")).ReturnsAsync((FirebaseIdentity?)null);

            var result = await _sut.ResetForgottenPasswordAsync(ConfirmDto());

            Assert.That(result.Error, Does.Contain("could not confirm"));
        }

        [Test]
        public async Task ResetForgottenPasswordAsync_ForUnknownBannedOrGoogleAccounts_GivesTheSameAnswer()
        {
            GivenValidResetProof();

            _customers.Setup(c => c.GetByEmail("alice@test.com")).ReturnsAsync((Customer?)null);
            var unknown = await _sut.ResetForgottenPasswordAsync(ConfirmDto());

            _customers.Setup(c => c.GetByEmail("alice@test.com"))
                .ReturnsAsync(new Customer { Id = 1, Email = "alice@test.com", Password = "hash:x", IsBanned = true });
            var banned = await _sut.ResetForgottenPasswordAsync(ConfirmDto());

            _customers.Setup(c => c.GetByEmail("alice@test.com"))
                .ReturnsAsync(new Customer { Id = 1, Email = "alice@test.com", Password = "" });
            var google = await _sut.ResetForgottenPasswordAsync(ConfirmDto());

            Assert.That(unknown.Error, Is.EqualTo(banned.Error));
            Assert.That(banned.Error, Is.EqualTo(google.Error));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task ResetForgottenPasswordAsync_WithoutProofOfTheEmailedLink_IsRejected()
        {
            GivenValidResetProof(verified: false);
            _customers.Setup(c => c.GetByEmail("alice@test.com"))
                .ReturnsAsync(new Customer { Id = 1, Email = "alice@test.com", Password = "hash:x" });

            var result = await _sut.ResetForgottenPasswordAsync(ConfirmDto());

            Assert.That(result.Succeeded, Is.False);
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task ResetForgottenPasswordAsync_WithValidProof_SetsTheNewPassword()
        {
            GivenValidResetProof();
            var user = new Customer { Id = 1, Email = "alice@test.com", Password = "hash:old" };
            _customers.Setup(c => c.GetByEmail("alice@test.com")).ReturnsAsync(user);

            var result = await _sut.ResetForgottenPasswordAsync(ConfirmDto());

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(user.Password, Is.EqualTo("hash:" + NewPassword));
            _customers.Verify(c => c.UpdateAsync(1, user), Times.Once);
        }

        // ---------- StartEmailChangeAsync ----------

        [Test]
        public async Task StartEmailChangeAsync_WithInvalidAddress_IsRejected()
        {
            GivenUser();

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "not-an-email" });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
        }

        [Test]
        public async Task StartEmailChangeAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task StartEmailChangeAsync_ForGoogleAccount_IsRejected()
        {
            GivenUser(password: null);

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Error, Does.Contain("Google"));
        }

        [Test]
        public async Task StartEmailChangeAsync_WithoutCurrentEmail_IsRejected()
        {
            GivenUser(email: "");

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Error, Does.Contain("no current email"));
        }

        [Test]
        public async Task StartEmailChangeAsync_ToTheSameAddress_IsRejected()
        {
            GivenUser();

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "ALICE@test.com" });

            Assert.That(result.Error, Does.Contain("already your email"));
        }

        [Test]
        public async Task StartEmailChangeAsync_ToAnAddressInUse_IsRejected()
        {
            GivenUser();
            _customers.Setup(c => c.GetByEmail("taken@test.com")).ReturnsAsync(new Customer { Id = 2 });

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "taken@test.com" });

            Assert.That(result.Error, Does.Contain("already in use"));
        }

        [Test]
        public async Task StartEmailChangeAsync_ReturnsAFirebaseTokenForTheClient()
        {
            GivenUser();
            _firebase.Setup(f => f.BeginEmailChangeAsync("alice@test.com", 1)).ReturnsAsync("firebase-token");

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "New@Test.com" });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Value!.FirebaseToken, Is.EqualTo("firebase-token"));
            _firebase.Verify(f => f.EnsureAccountAsync("alice@test.com"), Times.Once);
        }

        [Test]
        public async Task StartEmailChangeAsync_WhenFirebaseFails_ReturnsAFriendlyError()
        {
            GivenUser();
            _firebase.Setup(f => f.BeginEmailChangeAsync(It.IsAny<string>(), It.IsAny<int>())).ThrowsAsync(new Exception("down"));

            var result = await _sut.StartEmailChangeAsync(1, new StartEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
            Assert.That(result.Error, Does.Contain("could not start"));
        }

        // ---------- ConfirmEmailChangeAsync ----------

        [Test]
        public async Task ConfirmEmailChangeAsync_WithInvalidAddress_IsRejected()
        {
            GivenUser();

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "" });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.Invalid));
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_WhenAccountMissing_IsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Failure, Is.EqualTo(ProfileFailure.NotFound));
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_ForGoogleAccount_IsRejected()
        {
            GivenUser(password: null);

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Error, Does.Contain("Google"));
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_ToTheSameAddress_IsRejected()
        {
            GivenUser();

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "alice@test.com" });

            Assert.That(result.Error, Does.Contain("already your email"));
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_WhenFirebaseCheckThrows_ReturnsAFriendlyError()
        {
            GivenUser();
            _firebase.Setup(f => f.IsEmailChangeConfirmedAsync(It.IsAny<string>(), It.IsAny<int>())).ThrowsAsync(new Exception("down"));

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Error, Does.Contain("could not check"));
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_UntilTheLinkWasOpened_IsRejected()
        {
            GivenUser();
            _firebase.Setup(f => f.IsEmailChangeConfirmedAsync("new@test.com", 1)).ReturnsAsync(false);

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Error, Does.Contain("could not confirm"));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_WhenSomeoneTookTheAddressMeanwhile_IsRejected()
        {
            GivenUser();
            _firebase.Setup(f => f.IsEmailChangeConfirmedAsync("new@test.com", 1)).ReturnsAsync(true);
            _customers.Setup(c => c.GetByEmail("new@test.com")).ReturnsAsync(new Customer { Id = 2 });

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Error, Does.Contain("already in use"));
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_SwitchesTheEmailAndIssuesAFreshSession()
        {
            var user = GivenUser();
            _firebase.Setup(f => f.IsEmailChangeConfirmedAsync("new@test.com", 1)).ReturnsAsync(true);
            _customers.Setup(c => c.GetByEmail("new@test.com")).ReturnsAsync((Customer?)null);

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = " New@Test.com " });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Value!.AccessToken, Is.EqualTo("fresh"));
            Assert.That(user.Email, Is.EqualTo("new@test.com"));
            Assert.That(user.EmailVerified, Is.True);
            _customers.Verify(c => c.UpdateAsync(1, user), Times.Once);
            _firebase.Verify(f => f.CompleteEmailChangeAsync("new@test.com"), Times.Once);
            _refresh.Verify(r => r.CreateAuthResponseAsync(1, "new@test.com", "Alice", UserRole.Customer, null), Times.Once);
        }

        [Test]
        public async Task ConfirmEmailChangeAsync_EvenIfFirebaseCleanupFails_TheChangeStillSucceeds()
        {
            GivenUser();
            _firebase.Setup(f => f.IsEmailChangeConfirmedAsync("new@test.com", 1)).ReturnsAsync(true);
            _firebase.Setup(f => f.CompleteEmailChangeAsync(It.IsAny<string>())).ThrowsAsync(new Exception("down"));

            var result = await _sut.ConfirmEmailChangeAsync(1, new ConfirmEmailChangeDto { NewEmail = "new@test.com" });

            Assert.That(result.Succeeded, Is.True);
        }
    }
}
