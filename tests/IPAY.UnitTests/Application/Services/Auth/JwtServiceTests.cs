using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class JwtServiceTests
    {
        private Mock<IConfiguration> _configMock = null!;
        private JwtService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["Jwt:Key"]).Returns("this-is-a-sufficiently-long-test-signing-key-123");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("IPAY.UnitTests.Issuer");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("IPAY.UnitTests.Audience");

            _sut = new JwtService(_configMock.Object);
        }

        [Test]
        public void GenerateAccessToken_ReturnsNonEmptyToken()
        {
            var token = _sut.GenerateAccessToken("42", "user@example.com", UserRole.Customer);

            Assert.That(token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GenerateAccessToken_EmbedsUserIdAndEmailClaims()
        {
            var token = _sut.GenerateAccessToken("42", "user@example.com", UserRole.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo("42"));
            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.Email).Value, Is.EqualTo("user@example.com"));
        }

        [TestCase(UserRole.Customer)]
        [TestCase(UserRole.Seller)]
        [TestCase(UserRole.Admin)]
        public void GenerateAccessToken_EmbedsRoleClaim(UserRole role)
        {
            var token = _sut.GenerateAccessToken("42", "user@example.com", role);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value, Is.EqualTo(role.ToString()));
        }

        [Test]
        public void GenerateAccessToken_SetsConfiguredIssuerAndAudience()
        {
            var token = _sut.GenerateAccessToken("42", "user@example.com", UserRole.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Issuer, Is.EqualTo("IPAY.UnitTests.Issuer"));
            Assert.That(jwt.Audiences, Does.Contain("IPAY.UnitTests.Audience"));
        }

        [Test]
        public void GenerateAccessToken_SetsAnExpiryInTheFuture()
        {
            var token = _sut.GenerateAccessToken("42", "user@example.com", UserRole.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        }

        [Test]
        public void GenerateRefreshToken_ReturnsADifferentNonEmptyValueEachTime()
        {
            var first = _sut.GenerateRefreshToken();
            var second = _sut.GenerateRefreshToken();

            Assert.That(first, Is.Not.Null.And.Not.Empty);
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void HashToken_IsDeterministic_AndDoesNotReturnTheRawToken()
        {
            var raw = _sut.GenerateRefreshToken();
            var firstHash = _sut.HashToken(raw);
            var secondHash = _sut.HashToken(raw);

            Assert.That(secondHash, Is.EqualTo(firstHash));
            Assert.That(firstHash, Is.Not.EqualTo(raw));
        }
    }
}
