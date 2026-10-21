using System.Net;
using System.Net.Sockets;
using IPAY.Infrastructure.Email;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Email
{
    [TestFixture]
    public class SmtpEmailSenderTests
    {
        private static SmtpEmailSender Create(string host, string from, int port = 587, string username = "") =>
            new(Options.Create(new EmailOptions { Host = host, FromAddress = from, Port = port, Username = username }));

        /// <summary>A port nothing listens on, so connecting fails fast and offline.</summary>
        private static int GetUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

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
        public void SendAsync_WithInvalidRecipient_FailsWhileBuildingTheMessage()
        {
            // The host is never contacted: the address is parsed before any connection is opened.
            var sut = Create("smtp.invalid", "noreply@test.com");

            var ex = Assert.CatchAsync(() => sut.SendAsync("@@@", "Hi", "<p>x</p>"));

            Assert.That(ex, Is.InstanceOf<FormatException>());
        }

        [Test]
        public void SendAsync_WhenServerIsUnreachable_ThrowsAConnectionErrorNotAConfigurationError()
        {
            // Closed local port + a username, so the StartTls branch and the connect call are exercised offline.
            var sut = Create("127.0.0.1", "noreply@test.com", GetUnusedPort(), username: "user");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var ex = Assert.CatchAsync(() => sut.SendAsync("a@test.com", "Hi", "<p>x</p>", timeout.Token));

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex, Is.Not.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void SendAsync_OnImplicitSslPort_AlsoFailsCleanlyWhenNothingIsListening()
        {
            // Port 465 selects SslOnConnect instead of StartTls; against a closed/non-TLS endpoint it must still throw.
            var sut = Create("127.0.0.1", "noreply@test.com", port: 465);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var ex = Assert.CatchAsync(() => sut.SendAsync("a@test.com", "Hi", "<p>x</p>", timeout.Token));

            Assert.That(ex, Is.Not.InstanceOf<InvalidOperationException>());
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
