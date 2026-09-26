using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IAuthDto<RegisterDto>> _registerMock = null!;
        private Mock<ILogin> _loginMock = null!;
        private AuthController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _registerMock = new Mock<IAuthDto<RegisterDto>>();
            _loginMock = new Mock<ILogin>();
            _sut = new AuthController(_registerMock.Object, _loginMock.Object);
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
    }
}
