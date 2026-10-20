using System.Text.RegularExpressions;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Options;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class TwoFactorServiceTests
    {
        private sealed class FakeStore : ITwoFactorStore
        {
            public Dictionary<string, TwoFactorChallenge> Items { get; } = new();
            public void Save(TwoFactorChallenge challenge, TimeSpan lifetime) => Items[challenge.Id] = challenge;
            public TwoFactorChallenge? Get(string challengeId) => Items.TryGetValue(challengeId, out var c) ? c : null;
            public void Remove(string challengeId) => Items.Remove(challengeId);
        }

        private FakeStore _store = null!;
        private Mock<IEmailSender> _email = null!;
        private Mock<IUser<Customer>> _customers = null!;
        private Mock<IJWT> _jwt = null!;
        private Mock<IRefreshTokenService> _refresh = null!;
        private TwoFactorOptions _options = null!;
        private TwoFactorService _sut = null!;
        private readonly List<(string To, string Subject, string Body)> _sent = new();

        [SetUp]
        public void SetUp()
        {
            _sent.Clear();
            _store = new FakeStore();
            _email = new Mock<IEmailSender>();
            _email.SetupGet(e => e.IsConfigured).Returns(true);
            _email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, string, CancellationToken>((to, subj, body, _) => _sent.Add((to, subj, body)))
                .Returns(Task.CompletedTask);

            _customers = new Mock<IUser<Customer>>();
            _jwt = new Mock<IJWT>();
            _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>())).Returns("jwt");
            _refresh = new Mock<IRefreshTokenService>();
            _refresh.Setup(r => r.CreateAuthResponseAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<UserRole>(), It.IsAny<string?>()))
                .ReturnsAsync(new AuthResponse { AccessToken = "final-access", RefreshToken = "final-refresh" });

            _options = new TwoFactorOptions { Enabled = true, CodeLifetimeMinutes = 10, MaxAttempts = 3, ResendCooldownSeconds = 30 };
            _sut = new TwoFactorService(_store, _email.Object, _customers.Object, _jwt.Object, _options, _refresh.Object);
        }

        private static Customer User(int id = 1, string email = "alice@test.com") =>
            new() { Id = id, Email = email, Name = "Alice", Role = UserRole.Customer };

        private string LastSentCode()
        {
            var match = Regex.Match(_sent.Last().Body, @">(\d{6})<");
            Assert.That(match.Success, Is.True, "the email should contain the 6-digit code");
            return match.Groups[1].Value;
        }

        private async Task<(string ChallengeId, string Code)> StartChallenge(Customer? user = null)
        {
            user ??= User();
            _customers.Setup(c => c.GetByIdAsync(user.Id!.Value)).ReturnsAsync(user);
            var response = await _sut.BeginAsync(user);
            return (response.ChallengeId!, LastSentCode());
        }

        private static string WrongCode(string right) => right == "000000" ? "111111" : "000000";

        // ---------- IsEnabled ----------

        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void IsEnabled_NeedsBothTheSettingAndAConfiguredMailServer(bool enabled, bool configured, bool expected)
        {
            _options.Enabled = enabled;
            _email.SetupGet(e => e.IsConfigured).Returns(configured);

            Assert.That(_sut.IsEnabled, Is.EqualTo(expected));
        }

        // ---------- BeginAsync ----------

        [Test]
        public async Task BeginAsync_EmailsASixDigitCodeAndStoresAChallenge()
        {
            var response = await _sut.BeginAsync(User(email: "alice@test.com"));

            Assert.That(response.TwoFactorRequired, Is.True);
            Assert.That(response.ChallengeId, Is.Not.Null.And.Not.Empty);
            Assert.That(response.AccessToken, Is.Empty);
            Assert.That(_sent, Has.Count.EqualTo(1));
            Assert.That(_sent[0].To, Is.EqualTo("alice@test.com"));
            Assert.That(LastSentCode(), Has.Length.EqualTo(6));
            Assert.That(_store.Get(response.ChallengeId!), Is.Not.Null);
        }

        [Test]
        public async Task BeginAsync_MasksTheEmailAddress()
        {
            var response = await _sut.BeginAsync(User(email: "alice@test.com"));

            Assert.That(response.MaskedEmail, Is.EqualTo("a***@test.com"));
        }

        [Test]
        public async Task BeginAsync_NeverStoresTheCodeInPlainText()
        {
            await _sut.BeginAsync(User());

            var challenge = _store.Items.Values.Single();
            Assert.That(challenge.CodeHash, Is.Not.EqualTo(LastSentCode()));
            Assert.That(challenge.CodeHash, Has.Length.EqualTo(64));
        }

        [Test]
        public async Task BeginAsync_GivesEachChallengeItsOwnId()
        {
            var a = await _sut.BeginAsync(User());
            var b = await _sut.BeginAsync(User());

            Assert.That(a.ChallengeId, Is.Not.EqualTo(b.ChallengeId));
        }

        [Test]
        public void BeginAsync_WhenTheEmailCannotBeSent_ThrowsAndStoresNothing()
        {
            _email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("smtp down"));

            var ex = Assert.ThrowsAsync<TwoFactorDeliveryException>(() => _sut.BeginAsync(User()));

            Assert.That(ex!.InnerException, Is.InstanceOf<InvalidOperationException>());
            Assert.That(_store.Items, Is.Empty);
        }

        // ---------- VerifyAsync ----------

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("no-such-challenge")]
        public async Task VerifyAsync_WithMissingOrUnknownChallenge_SaysItExpired(string? id)
        {
            var result = await _sut.VerifyAsync(id!, "123456");

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Does.Contain("expired"));
        }

        [Test]
        public async Task VerifyAsync_WithExpiredChallenge_RemovesItAndFails()
        {
            var (id, code) = await StartChallenge();
            _store.Get(id)!.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1);

            var result = await _sut.VerifyAsync(id, code);

            Assert.That(result.Error, Does.Contain("expired"));
            Assert.That(_store.Get(id), Is.Null);
        }

        [TestCase("12345")]
        [TestCase("1234567")]
        [TestCase("abcdef")]
        [TestCase("")]
        public async Task VerifyAsync_WithMalformedCode_AsksForSixDigitsAndDoesNotCountAnAttempt(string code)
        {
            var (id, _) = await StartChallenge();

            var result = await _sut.VerifyAsync(id, code);

            Assert.That(result.Error, Does.Contain("6-digit"));
            Assert.That(_store.Get(id)!.FailedAttempts, Is.EqualTo(0));
        }

        [Test]
        public async Task VerifyAsync_WithWrongCode_CountsTheAttemptAndReportsHowManyAreLeft()
        {
            var (id, code) = await StartChallenge();

            var result = await _sut.VerifyAsync(id, WrongCode(code));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Does.Contain("2 attempts left"));
            Assert.That(_store.Get(id)!.FailedAttempts, Is.EqualTo(1));
        }

        [Test]
        public async Task VerifyAsync_OnTheSecondToLastAttempt_UsesTheSingularForm()
        {
            var (id, code) = await StartChallenge();

            await _sut.VerifyAsync(id, WrongCode(code));
            var result = await _sut.VerifyAsync(id, WrongCode(code));

            Assert.That(result.Error, Does.Contain("1 attempt left"));
            Assert.That(result.Error, Does.Not.Contain("attempts left"));
        }

        [Test]
        public async Task VerifyAsync_AfterTooManyWrongCodes_ThrowsTheChallengeAwayEvenForTheRightCode()
        {
            var (id, code) = await StartChallenge();

            await _sut.VerifyAsync(id, WrongCode(code));
            await _sut.VerifyAsync(id, WrongCode(code));
            var last = await _sut.VerifyAsync(id, WrongCode(code));
            var afterwards = await _sut.VerifyAsync(id, code);

            Assert.That(last.Error, Does.Contain("Too many"));
            Assert.That(_store.Get(id), Is.Null);
            Assert.That(afterwards.Succeeded, Is.False);
        }

        [Test]
        public async Task VerifyAsync_WithRightCode_ReturnsTheRealLoginResponse()
        {
            var (id, code) = await StartChallenge(User(id: 4, email: "alice@test.com"));

            var result = await _sut.VerifyAsync(id, code);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Value!.AccessToken, Is.EqualTo("final-access"));
            _refresh.Verify(r => r.CreateAuthResponseAsync(4, "alice@test.com", "Alice", UserRole.Customer, null), Times.Once);
        }

        [Test]
        public async Task VerifyAsync_TrimsWhitespaceAroundTheCode()
        {
            var (id, code) = await StartChallenge();

            var result = await _sut.VerifyAsync(id, $"  {code} ");

            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public async Task VerifyAsync_CodeCanOnlyBeUsedOnce()
        {
            var (id, code) = await StartChallenge();

            await _sut.VerifyAsync(id, code);
            var second = await _sut.VerifyAsync(id, code);

            Assert.That(second.Succeeded, Is.False);
            Assert.That(_store.Get(id), Is.Null);
        }

        [Test]
        public async Task VerifyAsync_WhenTheUserWasDeletedMeanwhile_Fails()
        {
            var (id, code) = await StartChallenge();
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

            var result = await _sut.VerifyAsync(id, code);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Does.Contain("Sign-in failed"));
        }

        [Test]
        public async Task VerifyAsync_WhenTheUserGotBannedMeanwhile_Fails()
        {
            var (id, code) = await StartChallenge();
            _customers.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1, Email = "alice@test.com", IsBanned = true });

            var result = await _sut.VerifyAsync(id, code);

            Assert.That(result.Succeeded, Is.False);
            _refresh.Verify(r => r.CreateAuthResponseAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<UserRole>(), It.IsAny<string?>()), Times.Never);
        }

        // ---------- ResendAsync ----------

        [TestCase(null)]
        [TestCase("")]
        [TestCase("unknown")]
        public async Task ResendAsync_WithMissingOrUnknownChallenge_Fails(string? id)
        {
            var result = await _sut.ResendAsync(id!);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Does.Contain("expired"));
        }

        [Test]
        public async Task ResendAsync_WithExpiredChallenge_Fails()
        {
            var (id, _) = await StartChallenge();
            _store.Get(id)!.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1);

            var result = await _sut.ResendAsync(id);

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task ResendAsync_InsideTheCooldown_TellsHowLongToWaitAndSendsNothing()
        {
            var (id, _) = await StartChallenge();

            var result = await _sut.ResendAsync(id);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Does.Contain("Please wait"));
            Assert.That(_sent, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task ResendAsync_AfterTheCooldown_SendsANewCodeThatReplacesTheOldOne()
        {
            var (id, oldCode) = await StartChallenge();
            _store.Get(id)!.LastSentAtUtc = DateTime.UtcNow.AddSeconds(-60);

            var result = await _sut.ResendAsync(id);
            var newCode = LastSentCode();

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Value, Is.EqualTo("a***@test.com"));
            Assert.That(_sent, Has.Count.EqualTo(2));

            // The fresh code works. (If the random codes happened to be identical both work - nothing to assert then.)
            if (newCode != oldCode)
            {
                var withOld = await _sut.VerifyAsync(id, oldCode);
                Assert.That(withOld.Succeeded, Is.False);
            }
            var withNew = await _sut.VerifyAsync(id, newCode);
            Assert.That(withNew.Succeeded, Is.True);
        }

        [Test]
        public async Task ResendAsync_DoesNotResetTheFailedAttemptCounter()
        {
            var (id, code) = await StartChallenge();
            await _sut.VerifyAsync(id, WrongCode(code));
            _store.Get(id)!.LastSentAtUtc = DateTime.UtcNow.AddSeconds(-60);

            await _sut.ResendAsync(id);

            Assert.That(_store.Get(id)!.FailedAttempts, Is.EqualTo(1));
        }

        [Test]
        public async Task ResendAsync_WhenEmailFails_Throws()
        {
            var (id, _) = await StartChallenge();
            _store.Get(id)!.LastSentAtUtc = DateTime.UtcNow.AddSeconds(-60);
            _email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("smtp down"));

            Assert.ThrowsAsync<TwoFactorDeliveryException>(() => _sut.ResendAsync(id));
        }
    }
}
