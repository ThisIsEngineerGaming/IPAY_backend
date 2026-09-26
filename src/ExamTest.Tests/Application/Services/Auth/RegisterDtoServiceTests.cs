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
    public class RegisterDtoServiceTests
    {
        private Mock<IValidator<RegisterDto>> _validatorMock = null!;
        private Mock<IUser<Seller>> _sellerServiceMock = null!;
        private Mock<IPasswordHashingService> _passwordHasherMock = null!;
        private RegisterDtoService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<RegisterDto>>();
            _sellerServiceMock = new Mock<IUser<Seller>>();
            _passwordHasherMock = new Mock<IPasswordHashingService>();

            _sut = new RegisterDtoService(_validatorMock.Object, _sellerServiceMock.Object, _passwordHasherMock.Object);
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
        public async Task Register_WhenValidationFails_ReturnsFalseAndDoesNotCreateSeller()
        {
            SetupValidation(isValid: false);

            var result = await _sut.Register(new RegisterDto { Email = "a@test.com", Password = "x", ConfirmPassword = "x", Name = "A" });

            Assert.That(result, Is.False);
            _sellerServiceMock.Verify(s => s.CreateAsync(It.IsAny<Seller>()), Times.Never);
        }

        [Test]
        public async Task Register_WhenSellerCreationSucceeds_ReturnsTrue()
        {
            SetupValidation(isValid: true);
            _passwordHasherMock.Setup(p => p.Hash("Sup3r$ecret!")).Returns("hashed-password");
            _sellerServiceMock
                .Setup(s => s.CreateAsync(It.IsAny<Seller>()))
                .ReturnsAsync((Seller s) => s);

            var result = await _sut.Register(new RegisterDto
            {
                Email = "  New@Test.com  ",
                Password = "Sup3r$ecret!",
                ConfirmPassword = "Sup3r$ecret!",
                Name = "  New Seller  "
            });

            Assert.That(result, Is.True);
            _sellerServiceMock.Verify(s => s.CreateAsync(It.Is<Seller>(seller =>
                seller.Email == "new@test.com" &&
                seller.Name == "New Seller" &&
                seller.Password == "hashed-password")), Times.Once);
        }

        [Test]
        public async Task Register_WhenSellerServiceReturnsNull_ReturnsFalse()
        {
            SetupValidation(isValid: true);
            _passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
            _sellerServiceMock.Setup(s => s.CreateAsync(It.IsAny<Seller>())).ReturnsAsync((Seller?)null);

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
