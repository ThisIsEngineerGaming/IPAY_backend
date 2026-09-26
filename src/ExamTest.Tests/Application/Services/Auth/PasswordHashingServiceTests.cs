using ExamTest.Application.Services.Auth;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Auth
{
    [TestFixture]
    public class PasswordHashingServiceTests
    {
        private PasswordHashingService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _sut = new PasswordHashingService();
        }

        [Test]
        public void Hash_ReturnsNonEmptyStringWithFourPartsSeparatedByDollarSign()
        {
            var hash = _sut.Hash("Sup3r$ecret!");

            Assert.That(hash, Is.Not.Null.And.Not.Empty);
            Assert.That(hash.Split('$'), Has.Length.EqualTo(4));
        }

        [Test]
        public void Hash_ProducesDifferentOutputForTheSamePasswordEachTime()
        {
            var first = _sut.Hash("Sup3r$ecret!");
            var second = _sut.Hash("Sup3r$ecret!");

            // Salts are randomly generated per call, so two hashes of the same password
            // should never be identical - if they were, salting would be broken.
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void Verify_WithCorrectPassword_ReturnsTrue()
        {
            var hash = _sut.Hash("Sup3r$ecret!");

            var result = _sut.Verify("Sup3r$ecret!", hash);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Verify_WithIncorrectPassword_ReturnsFalse()
        {
            var hash = _sut.Hash("Sup3r$ecret!");

            var result = _sut.Verify("WrongPassword!", hash);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Verify_WithMalformedHash_ReturnsFalseInsteadOfThrowing()
        {
            var result = _sut.Verify("Sup3r$ecret!", "not-a-real-hash");

            Assert.That(result, Is.False);
        }

        [Test]
        public void Verify_WithUnsupportedAlgorithmTag_ReturnsFalse()
        {
            var result = _sut.Verify("Sup3r$ecret!", "MD5$1$c2FsdA==$aGFzaA==");

            Assert.That(result, Is.False);
        }

        [Test]
        public void Verify_WithInvalidBase64Segments_ReturnsFalse()
        {
            var result = _sut.Verify("Sup3r$ecret!", "PBKDF2-SHA256$210000$not-base64$not-base64");

            Assert.That(result, Is.False);
        }
    }
}
