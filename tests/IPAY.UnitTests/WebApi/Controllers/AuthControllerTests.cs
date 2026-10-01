using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IAuthDto<RegisterDto>> _registerMock = null!;
        private Mock<ILogin> _loginMock = null!;
        private Mock<IUser<Customer>> _customerMock = null!;
        private AuthController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _registerMock = new Mock<IAuthDto<RegisterDto>>();
            _loginMock = new Mock<ILogin>();
            _customerMock = new Mock<IUser<Customer>>();
            _sut = new AuthController(_registerMock.Object, _loginMock.Object, _customerMock.Object);
        }

        [Test]
        public async Task Registration_WhenRegisterSucceeds_ReturnsOk()
        {
            _registerMock.Setup(r => r.Register(It.IsAny<RegisterDto>())).ReturnsAsync(true);

            var result = await _sut.Registration(new RegisterDto { Email = "a@test.com", Password = "x", ConfirmPassword = "x", Name = "A" });

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Registration_WhenRegisterFails_ReturnsBadRequest()
        {
            _registerMock.Setup(r => r.Register(It.IsAny<RegisterDto>())).ReturnsAsync(false);

            var result = await _sut.Registration(new RegisterDto { Email = "bad", Password = "x", ConfirmPassword = "y", Name = "A" });

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Login_WhenCredentialsValid_ReturnsOkWithAuthResponse()
        {
            var response = new AuthResponse { AccessToken = "token", User = new LoginDto { Email = "a@test.com" } };
            _loginMock.Setup(l => l.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync(response);

            var result = await _sut.Login(new LoginDto { Email = "a@test.com", Password = "password1" });

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(((AuthResponse)okResult!.Value!).AccessToken, Is.EqualTo("token"));
        }

        [Test]
        public async Task Login_WhenCredentialsInvalid_ReturnsBadRequest()
        {
            _loginMock.Setup(l => l.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync((AuthResponse?)null);

            var result = await _sut.Login(new LoginDto { Email = "a@test.com", Password = "wrong" });

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ChangeRole_WhenUserExists_UpdatesRoleAndReturnsOk()
        {
            var customer = new Customer { Id = 5, Email = "a@test.com", Role = UserRole.Customer };
            _customerMock.Setup(c => c.GetByIdAsync(5)).ReturnsAsync(customer);

            var result = await _sut.ChangeRole(5, UserRole.Admin);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _customerMock.Verify(c => c.UpdateAsync(5, It.Is<Customer>(u => u.Role == UserRole.Admin)), Times.Once);
        }

        [Test]
        public async Task ChangeRole_WhenUserMissing_ReturnsNotFoundAndDoesNotUpdate()
        {
            _customerMock.Setup(c => c.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);

            var result = await _sut.ChangeRole(404, UserRole.Admin);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _customerMock.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task MakeRequest_WhenUserExists_PromotesToSeller()
        {
            var customer = new Customer { Id = 3, Role = UserRole.Customer };
            _customerMock.Setup(c => c.GetByIdAsync(3)).ReturnsAsync(customer);

            var result = await _sut.MakeRequest(3);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _customerMock.Verify(c => c.UpdateAsync(3, It.Is<Customer>(u => u.Role == UserRole.Seller)), Times.Once);
        }

        [Test]
        public async Task MakeRequest_WhenUserMissing_ReturnsNotFound()
        {
            _customerMock.Setup(c => c.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);

            var result = await _sut.MakeRequest(404);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
