using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class LoginDtoServiceTests
    {
        private Mock<IValidator<LoginDto>> _validatorMock = null!;
        private Mock<IJWT> _jwtMock = null!;
        private Mock<IUser<Customer>> _customerMock = null!;
        private Mock<IPasswordHashingService> _passwordHasherMock = null!;
        private LoginDtoService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<LoginDto>>();
            _jwtMock = new Mock<IJWT>();
            _customerMock = new Mock<IUser<Customer>>();
            _passwordHasherMock = new Mock<IPasswordHashingService>();

            _sut = new LoginDtoService(_validatorMock.Object, _jwtMock.Object, _customerMock.Object, _passwordHasherMock.Object);
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
            _customerMock.Verify(s => s.GetByEmail(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task LoginAsync_WhenUserNotFound_ReturnsNull()
        {
            SetupValidation(isValid: true);
            _customerMock.Setup(s => s.GetByEmail("missing@test.com")).ReturnsAsync((Customer?)null);

            var result = await _sut.LoginAsync(new LoginDto { Email = "missing@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WhenUserHasNoIdOrEmail_ReturnsNull()
        {
            SetupValidation(isValid: true);
            _customerMock.Setup(s => s.GetByEmail(It.IsAny<string>())).ReturnsAsync(new Customer { Id = null, Email = "a@test.com" });

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WhenPasswordDoesNotMatch_ReturnsNull()
        {
            SetupValidation(isValid: true);
            var customer = new Customer { Id = 1, Email = "a@test.com", Password = "hashed", Name = "Alice", Role = UserRole.Customer };
            _customerMock.Setup(s => s.GetByEmail("a@test.com")).ReturnsAsync(customer);
            _passwordHasherMock.Setup(p => p.Verify("password1", "hashed")).Returns(false);

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WhenCredentialsValid_ReturnsAuthResponseWithToken()
        {
            SetupValidation(isValid: true);
            var customer = new Customer { Id = 1, Email = "a@test.com", Password = "hashed", Name = "Alice", Role = UserRole.Customer };
            _customerMock.Setup(s => s.GetByEmail("a@test.com")).ReturnsAsync(customer);
            _passwordHasherMock.Setup(p => p.Verify("password1", "hashed")).Returns(true);
            _jwtMock.Setup(j => j.GenerateToken("1", "a@test.com", UserRole.Customer)).Returns("jwt-token");

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AccessToken, Is.EqualTo("jwt-token"));
            Assert.That(result.TokenType, Is.EqualTo("Bearer"));
            Assert.That(result.User.Email, Is.EqualTo("a@test.com"));
            Assert.That(result.User.Name, Is.EqualTo("Alice"));
        }

        [TestCase(UserRole.Customer)]
        [TestCase(UserRole.Seller)]
        [TestCase(UserRole.Admin)]
        public async Task LoginAsync_PassesUserRoleToJwt(UserRole role)
        {
            SetupValidation(isValid: true);
            var customer = new Customer { Id = 7, Email = "r@test.com", Password = "hashed", Role = role };
            _customerMock.Setup(s => s.GetByEmail("r@test.com")).ReturnsAsync(customer);
            _passwordHasherMock.Setup(p => p.Verify("password1", "hashed")).Returns(true);
            _jwtMock.Setup(j => j.GenerateToken("7", "r@test.com", role)).Returns("jwt-" + role);

            var result = await _sut.LoginAsync(new LoginDto { Email = "r@test.com", Password = "password1" });

            Assert.That(result!.AccessToken, Is.EqualTo("jwt-" + role));
        }

        [Test]
        public async Task LoginAsync_WhenCustomerPasswordMissing_ReturnsNull()
        {
            SetupValidation(isValid: true);
            var customer = new Customer { Id = 1, Email = "a@test.com", Password = "" };
            _customerMock.Setup(s => s.GetByEmail("a@test.com")).ReturnsAsync(customer);

            var result = await _sut.LoginAsync(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(result, Is.Null);
            _passwordHasherMock.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
