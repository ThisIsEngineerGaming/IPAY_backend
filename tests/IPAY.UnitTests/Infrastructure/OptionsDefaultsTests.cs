using IPAY.Application.Options;
using IPAY.Infrastructure.Cloudinary;
using IPAY.Infrastructure.Persistence;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure
{
    [TestFixture]
    public class OptionsDefaultsTests
    {
        [Test]
        public void TwoFactorOptions_DefaultToTenMinutesFiveTriesAndThirtySecondsCooldown()
        {
            var options = new TwoFactorOptions();

            Assert.That(options.Enabled, Is.True);
            Assert.That(options.CodeLifetimeMinutes, Is.EqualTo(10));
            Assert.That(options.MaxAttempts, Is.EqualTo(5));
            Assert.That(options.ResendCooldownSeconds, Is.EqualTo(30));
            Assert.That(TwoFactorOptions.SectionName, Is.EqualTo("TwoFactor"));
        }

        [Test]
        public void FirebaseOptions_RequireEmailVerificationByDefault()
        {
            var options = new FirebaseOptions();

            Assert.That(options.RequireEmailVerification, Is.True);
            Assert.That(FirebaseOptions.SectionName, Is.EqualTo("Firebase"));
        }

        [Test]
        public void CloudinaryOptions_StartEmptyAndHaveTheirConfigSectionName()
        {
            var options = new CloudinaryOptions();

            Assert.That(options.CloudName, Is.Empty);
            Assert.That(options.ApiSecret, Is.Empty);
            Assert.That(CloudinaryOptions.SectionName, Is.EqualTo("Cloudinary"));
        }
    }
}
