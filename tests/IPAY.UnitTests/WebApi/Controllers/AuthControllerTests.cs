using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
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
        private Mock<IGoogleLogin> _googleMock = null!;
        private Mock<ITwoFactorService> _twoFactorMock = null!;
        private AuthController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _registerMock = new Mock<IAuthDto<RegisterDto>>();
            _loginMock = new Mock<ILogin>();
            _customerMock = new Mock<IUser<Customer>>();
            _googleMock = new Mock<IGoogleLogin>();
            _twoFactorMock = new Mock<ITwoFactorService>();
            _sut = new AuthController(
                _registerMock.Object, _loginMock.Object, _customerMock.Object,
                _googleMock.Object, _twoFactorMock.Object);
        }

        // ---------- register / login ----------

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
        public async Task Login_WhenEmailNotVerified_Returns403()
        {
            _loginMock.Setup(l => l.LoginAsync(It.IsAny<LoginDto>())).ThrowsAsync(new EmailNotVerifiedException());

            var result = await _sut.Login(new LoginDto { Email = "a@test.com", Password = "password1" });

            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        }

        [Test]
        public async Task Login_WhenCodeEmailCannotBeSent_Returns503()
        {
            _loginMock.Setup(l => l.LoginAsync(It.IsAny<LoginDto>()))
                .ThrowsAsync(new TwoFactorDeliveryException(new Exception("smtp down")));

            var result = await _sut.Login(new LoginDto { Email = "a@test.com", Password = "password1" });

            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
        }

        // ---------- two-factor ----------

        [Test]
        public async Task VerifyCode_WhenCodeCorrect_ReturnsOkWithSession()
        {
            var session = new AuthResponse { AccessToken = "jwt" };
            _twoFactorMock.Setup(t => t.VerifyAsync("chal", "123456"))
                .ReturnsAsync(TwoFactorResult<AuthResponse>.Ok(session));

            var result = await _sut.VerifyCode(new VerifyCodeDto { ChallengeId = "chal", Code = "123456" });

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(session));
        }

        [Test]
        public async Task VerifyCode_WhenCodeWrong_ReturnsBadRequestNot401()
        {
            // 400 on purpose: the frontend redirects to /login on any 401.
            _twoFactorMock.Setup(t => t.VerifyAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(TwoFactorResult<AuthResponse>.Fail("Wrong code"));

            var result = await _sut.VerifyCode(new VerifyCodeDto { ChallengeId = "chal", Code = "000000" });

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResendCode_WhenAllowed_ReturnsOk()
        {
            _twoFactorMock.Setup(t => t.ResendAsync("chal")).ReturnsAsync(TwoFactorResult<string>.Ok("a***@test.com"));

            var result = await _sut.ResendCode(new ResendCodeDto { ChallengeId = "chal" });

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ResendCode_WhenRateLimited_ReturnsBadRequest()
        {
            _twoFactorMock.Setup(t => t.ResendAsync("chal")).ReturnsAsync(TwoFactorResult<string>.Fail("Wait 30 seconds"));

            var result = await _sut.ResendCode(new ResendCodeDto { ChallengeId = "chal" });

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ---------- Google ----------

        [Test]
        public async Task GoogleLogin_WhenTokenAccepted_ReturnsOk()
        {
            _googleMock.Setup(g => g.LoginWithGoogleAsync("id-token")).ReturnsAsync(new AuthResponse { AccessToken = "jwt" });

            var result = await _sut.GoogleLogin(new GoogleLoginDto { IdToken = "id-token" });

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GoogleLogin_WhenTokenRejected_ReturnsBadRequest()
        {
            _googleMock.Setup(g => g.LoginWithGoogleAsync(It.IsAny<string>())).ReturnsAsync((AuthResponse?)null);

            var result = await _sut.GoogleLogin(new GoogleLoginDto { IdToken = "bad" });

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GoogleLogin_WhenServiceCrashes_Returns500WithoutLeakingDetails()
        {
            _googleMock.Setup(g => g.LoginWithGoogleAsync(It.IsAny<string>())).ThrowsAsync(new Exception("firestore secret detail"));

            var result = await _sut.GoogleLogin(new GoogleLoginDto { IdToken = "x" });

            var objectResult = (ObjectResult)result.Result!;
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(objectResult.Value!.ToString(), Does.Not.Contain("secret"));
        }

        // ---------- seller request ----------

        [Test]
        public async Task MakeRequest_TakesTheUserFromTheTokenAndPromotesToSeller()
        {
            var customer = new Customer { Id = 3, Role = UserRole.Customer };
            _customerMock.Setup(c => c.GetByIdAsync(3)).ReturnsAsync(customer);
            _sut.WithUser(3, "Customer");

            var result = await _sut.MakeRequest();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _customerMock.Verify(c => c.UpdateAsync(3, It.Is<Customer>(u => u.Role == UserRole.Seller)), Times.Once);
        }

        [Test]
        public async Task MakeRequest_WhenUserRecordMissing_ReturnsNotFound()
        {
            _customerMock.Setup(c => c.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);
            _sut.WithUser(404, "Customer");

            var result = await _sut.MakeRequest();

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _customerMock.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task MakeRequest_WhenTokenHasNoUserId_ReturnsUnauthorized()
        {
            _sut.WithUser(id: null, "Customer");

            var result = await _sut.MakeRequest();

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }
    }
}
