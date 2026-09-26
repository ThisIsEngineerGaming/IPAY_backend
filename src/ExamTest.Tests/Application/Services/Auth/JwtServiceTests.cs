using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExamTest.Application.Services.Auth;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Auth
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
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("ExamTest.Tests.Issuer");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("ExamTest.Tests.Audience");

            _sut = new JwtService(_configMock.Object);
        }

        [Test]
        public void GenerateToken_ReturnsNonEmptyToken()
        {
            var token = _sut.GenerateToken("42", "user@example.com");

            Assert.That(token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GenerateToken_EmbedsUserIdAndEmailClaims()
        {
            var token = _sut.GenerateToken("42", "user@example.com");

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo("42"));
            Assert.That(jwt.Claims.Single(c => c.Type == ClaimTypes.Email).Value, Is.EqualTo("user@example.com"));
        }

        [Test]
        public void GenerateToken_SetsConfiguredIssuerAndAudience()
        {
            var token = _sut.GenerateToken("42", "user@example.com");

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Issuer, Is.EqualTo("ExamTest.Tests.Issuer"));
            Assert.That(jwt.Audiences, Does.Contain("ExamTest.Tests.Audience"));
        }

        [Test]
        public void GenerateToken_SetsAnExpiryInTheFuture()
        {
            var token = _sut.GenerateToken("42", "user@example.com");

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        }
    }
}
