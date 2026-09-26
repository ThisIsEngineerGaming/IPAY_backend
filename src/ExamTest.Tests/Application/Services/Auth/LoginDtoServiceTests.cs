using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using ExamTest.Application.Services.Auth;
using ExamTest.Domain.Entities.Auth;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Auth
{
    [TestFixture]
    public class LoginDtoServiceTests
    {
        private Mock<IValidator<LoginDto>> _validatorMock = null!;
        private Mock<IJWT> _jwtMock = null!;
        private Mock<IUser<Seller>> _sellerMock = null!;
        private Mock<IPasswordHashingService> _passwordHasherMock = null!;
        private LoginDtoService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<LoginDto>>();
            _jwtMock = new Mock<IJWT>();
            _sellerMock = new Mock<IUser<Seller>>();
            _passwordHasherMock = new Mock<IPasswordHashingService>();

            _sut = new LoginDtoService(_validatorMock.Object, _jwtMock.Object, _sellerMock.Object, _passwordHasherMock.Object);
        }

        private void SetupValidation(bool isValid)
        {
            var result = isValid
                ? new ValidationResult()
                : new ValidationResult(new List<ValidationFailure> { new("Email", "Invalid email") });

            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        [Test]
        public async Task Login_WhenValidatorPasses_ReturnsTrue()
        {
            SetupValidation(isValid: true);

            var result = await _sut.Login(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Login_WhenValidatorFails_ReturnsFalse()
        {
            SetupValidation(isValid: false);

            var result = await _sut.Login(new LoginDto { Email = "not-an-email", Password = "x" });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task LoginAsync_WhenValidationFails_ReturnsNull()
        {
            SetupValidation(isValid: false);

            var result = await _sut.LoginAsync(new LoginDto { Email = "bad", Password = "x" });

            Assert.That(result, Is.Null);
            _sellerMock.Verify(s => s.GetByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task LoginAsync_WhenUserNotFound_ReturnsNull()
        {
            SetupValidation(isValid: true);
            _sellerMock.Setup(s => s.GetByEmail("missing@test.com")).ReturnsAsync((Seller?)null);

            var result = await _sut.LoginAsync(new LoginDto { Email = "missing@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WhenUserHasNoIdOrEmail_ReturnsNull()
        {
            SetupValidation(isValid: true);
            _sellerMock.Setup(s => s.GetByEmail(It.IsAny<string>())).ReturnsAsync(new Seller { Id = null, Email = "a@test.com" });

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WhenPasswordDoesNotMatch_ReturnsNull()
        {
            SetupValidation(isValid: true);
            var seller = new Seller { Id = 1, Email = "a@test.com", Password = "hashed" };
            _sellerMock.Setup(s => s.GetByEmail("a@test.com")).ReturnsAsync(seller);
            _passwordHasherMock.Setup(p => p.Verify("password1", "hashed")).Returns(false);

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WhenCredentialsValid_ReturnsAuthResponseWithToken()
        {
            SetupValidation(isValid: true);
            var seller = new Seller { Id = 1, Email = "a@test.com", Password = "hashed" };
            _sellerMock.Setup(s => s.GetByEmail("a@test.com")).ReturnsAsync(seller);
            _passwordHasherMock.Setup(p => p.Verify("password1", "hashed")).Returns(true);
            _jwtMock.Setup(j => j.GenerateToken("1", "a@test.com")).Returns("jwt-token");

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AccessToken, Is.EqualTo("jwt-token"));
            Assert.That(result.TokenType, Is.EqualTo("Bearer"));
            Assert.That(result.User.Email, Is.EqualTo("a@test.com"));
        }

        [Test]
        public async Task LoginAsync_WhenSellerPasswordMissing_ReturnsNull()
        {
            SetupValidation(isValid: true);
            var seller = new Seller { Id = 1, Email = "a@test.com", Password = "" };
            _sellerMock.Setup(s => s.GetByEmail("a@test.com")).ReturnsAsync(seller);

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
            _passwordHasherMock.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
