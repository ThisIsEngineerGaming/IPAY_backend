using IPAY.Infrastructure.Email;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Email
{
    [TestFixture]
    public class SmtpEmailSenderTests
    {
        private static SmtpEmailSender Create(string host, string from) =>
            new(Options.Create(new EmailOptions { Host = host, FromAddress = from }));

        [Test]
        public void IsConfigured_NeedsBothAHostAndAFromAddress()
        {
            Assert.That(Create("smtp.test.com", "noreply@test.com").IsConfigured, Is.True);
            Assert.That(Create("", "noreply@test.com").IsConfigured, Is.False);
            Assert.That(Create("smtp.test.com", "").IsConfigured, Is.False);
            Assert.That(Create("  ", "  ").IsConfigured, Is.False);
        }

        [Test]
        public void SendAsync_WhenNotConfigured_ThrowsBeforeTryingToConnect()
        {
            var sut = Create("", "");

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => sut.SendAsync("a@test.com", "Hi", "<p>x</p>"));

            Assert.That(ex!.Message, Does.Contain("not configured"));
        }

        [Test]
        public void EmailOptions_HaveSensibleDefaults()
        {
            var options = new EmailOptions();

            Assert.That(options.Port, Is.EqualTo(587));
            Assert.That(options.FromName, Is.EqualTo("IPAY"));
            Assert.That(options.Host, Is.Empty);
        }
    }
}
