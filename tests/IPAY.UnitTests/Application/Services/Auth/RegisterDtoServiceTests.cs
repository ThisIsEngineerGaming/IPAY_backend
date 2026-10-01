using AutoMapper;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Mappings.Auth;
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
    public class RegisterDtoServiceTests
    {
        private Mock<IValidator<RegisterDto>> _validatorMock = null!;
        private Mock<IUser<Customer>> _customerServiceMock = null!;
        private Mock<IPasswordHashingService> _passwordHasherMock = null!;
        private RegisterDtoService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<RegisterDto>>();
            _customerServiceMock = new Mock<IUser<Customer>>();
            _passwordHasherMock = new Mock<IPasswordHashingService>();

            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<AuthMapping>()).CreateMapper();

            _sut = new RegisterDtoService(_validatorMock.Object, _customerServiceMock.Object, _passwordHasherMock.Object, mapper);
        }

        private void SetupValidation(bool isValid)
        {
            var result = isValid
                ? new ValidationResult()
                : new ValidationResult(new List<ValidationFailure> { new("Password", "Too weak") });

            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        [Test]
        public async Task Register_WhenValidationFails_ReturnsFalseAndDoesNotCreateCustomer()
        {
            SetupValidation(isValid: false);

            var result = await _sut.Register(new RegisterDto { Email = "a@test.com", Password = "x", ConfirmPassword = "x", Name = "A" });

            Assert.That(result, Is.False);
            _customerServiceMock.Verify(s => s.CreateAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task Register_WhenCustomerCreationSucceeds_ReturnsTrue()
        {
            SetupValidation(isValid: true);
            _passwordHasherMock.Setup(p => p.Hash("Sup3r$ecret!")).Returns("hashed-password");
            _customerServiceMock
                .Setup(s => s.CreateAsync(It.IsAny<Customer>()))
                .ReturnsAsync((Customer s) => s);

            var result = await _sut.Register(new RegisterDto
            {
                Email = "  New@Test.com  ",
                Password = "Sup3r$ecret!",
                ConfirmPassword = "Sup3r$ecret!",
                Name = "  New Customer  "
            });

            Assert.That(result, Is.True);
            _customerServiceMock.Verify(s => s.CreateAsync(It.Is<Customer>(customer =>
                customer.Email == "new@test.com" &&
                customer.Name == "New Customer" &&
                customer.Password == "hashed-password")), Times.Once);
        }

        [Test]
        public async Task Register_AlwaysCreatesACustomerRole_NeverSellerOrAdmin()
        {
            SetupValidation(isValid: true);
            _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
            _customerServiceMock
                .Setup(s => s.CreateAsync(It.IsAny<Customer>()))
                .ReturnsAsync((Customer c) => c);

            await _sut.Register(new RegisterDto
            {
                Email = "new@test.com",
                Password = "Sup3r$ecret!",
                ConfirmPassword = "Sup3r$ecret!",
                Name = "New Customer"
            });

            _customerServiceMock.Verify(s => s.CreateAsync(It.Is<Customer>(c => c.Role == UserRole.Customer)), Times.Once);
        }

        [Test]
        public async Task Register_WhenCustomerServiceReturnsNull_ReturnsFalse()
        {
            SetupValidation(isValid: true);
            _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
            _customerServiceMock.Setup(s => s.CreateAsync(It.IsAny<Customer>())).ReturnsAsync((Customer?)null);

            var result = await _sut.Register(new RegisterDto
            {
                Email = "taken@test.com",
                Password = "Sup3r$ecret!",
                ConfirmPassword = "Sup3r$ecret!",
                Name = "Someone"
            });

            Assert.That(result, Is.False);
        }
    }
}
