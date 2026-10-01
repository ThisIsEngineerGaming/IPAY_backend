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
        public void GenerateToken_ReturnsNonEmptyToken()
        {
            var token = _sut.GenerateToken("42", "user@example.com", UserRole.Customer);

            Assert.That(token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GenerateToken_EmbedsUserIdAndEmailClaims()
        {
            var token = _sut.GenerateToken("42", "user@example.com", UserRole.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo("42"));
            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.Email).Value, Is.EqualTo("user@example.com"));
        }

        [TestCase(UserRole.Customer)]
        [TestCase(UserRole.Seller)]
        [TestCase(UserRole.Admin)]
        public void GenerateToken_EmbedsRoleClaim(UserRole role)
        {
            var token = _sut.GenerateToken("42", "user@example.com", role);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value, Is.EqualTo(role.ToString()));
        }

        [Test]
        public void GenerateToken_SetsConfiguredIssuerAndAudience()
        {
            var token = _sut.GenerateToken("42", "user@example.com", UserRole.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Issuer, Is.EqualTo("IPAY.UnitTests.Issuer"));
            Assert.That(jwt.Audiences, Does.Contain("IPAY.UnitTests.Audience"));
        }

        [Test]
        public void GenerateToken_SetsAnExpiryInTheFuture()
        {
            var token = _sut.GenerateToken("42", "user@example.com", UserRole.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        }
    }
}
