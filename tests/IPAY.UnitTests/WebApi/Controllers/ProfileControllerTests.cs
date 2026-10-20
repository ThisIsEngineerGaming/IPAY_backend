using System.Security.Claims;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class ProfileControllerTests
    {
        private Mock<IProfileService> _service = null!;
        private ProfileController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IProfileService>();
            _sut = new ProfileController(_service.Object).WithUser(8, "Customer");
        }

        private static ProfileResult<ProfileDto> OkProfile() =>
            ProfileResult<ProfileDto>.Ok(new ProfileDto { Name = "Alice" });

        // ---------- result mapping ----------

        [Test]
        public async Task Get_OnSuccess_ReturnsTheProfile()
        {
            _service.Setup(s => s.GetProfileAsync(8)).ReturnsAsync(OkProfile());

            var result = await _sut.Get();

            Assert.That(((ProfileDto)((OkObjectResult)result).Value!).Name, Is.EqualTo("Alice"));
        }

        [Test]
        public async Task Get_WhenAccountIsGone_Returns404()
        {
            _service.Setup(s => s.GetProfileAsync(8)).ReturnsAsync(ProfileResult<ProfileDto>.NotFound());

            Assert.That(await _sut.Get(), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task ChangeUsername_WhenInvalid_Returns400NotAnUnauthorised()
        {
            // A 401 would make the frontend log the user out; "wrong password" must be a 400.
            var dto = new ChangeUsernameDto { NewName = "Bob", Password = "x" };
            _service.Setup(s => s.ChangeUsernameAsync(8, dto)).ReturnsAsync(ProfileResult<ProfileDto>.Fail("Incorrect password."));

            var result = await _sut.ChangeUsername(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Incorrect password."));
        }

        // ---------- every action uses the id from the token ----------

        [Test]
        public async Task ChangeUsername_PassesTheTokenUserAndBody()
        {
            var dto = new ChangeUsernameDto { NewName = "Bob" };
            _service.Setup(s => s.ChangeUsernameAsync(8, dto)).ReturnsAsync(OkProfile());

            Assert.That(await _sut.ChangeUsername(dto), Is.InstanceOf<OkObjectResult>());
            _service.Verify(s => s.ChangeUsernameAsync(8, dto), Times.Once);
        }

        [Test]
        public async Task ChangePhone_PassesTheTokenUserAndBody()
        {
            var dto = new ChangePhoneDto { NewPhone = "+380123456789" };
            _service.Setup(s => s.ChangePhoneAsync(8, dto)).ReturnsAsync(OkProfile());

            Assert.That(await _sut.ChangePhone(dto), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ChangePassword_PassesTheTokenUserAndBody()
        {
            var dto = new ChangePasswordDto();
            _service.Setup(s => s.ChangePasswordAsync(8, dto)).ReturnsAsync(OkProfile());

            Assert.That(await _sut.ChangePassword(dto), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task StartPasswordChange_UsesTheTokenUser()
        {
            _service.Setup(s => s.StartPasswordChangeAsync(8)).ReturnsAsync(OkProfile());

            Assert.That(await _sut.StartPasswordChange(), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ConfirmPasswordChange_PassesTheTokenUserAndBody()
        {
            var dto = new ConfirmPasswordChangeDto();
            _service.Setup(s => s.ConfirmPasswordChangeAsync(8, dto)).ReturnsAsync(OkProfile());

            Assert.That(await _sut.ConfirmPasswordChange(dto), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task StartEmailChange_ReturnsTheFirebaseToken()
        {
            var dto = new StartEmailChangeDto { NewEmail = "new@test.com" };
            _service.Setup(s => s.StartEmailChangeAsync(8, dto))
                .ReturnsAsync(ProfileResult<StartEmailChangeResultDto>.Ok(new StartEmailChangeResultDto { FirebaseToken = "t" }));

            var result = await _sut.StartEmailChange(dto);

            Assert.That(((StartEmailChangeResultDto)((OkObjectResult)result).Value!).FirebaseToken, Is.EqualTo("t"));
        }

        [Test]
        public async Task ConfirmEmailChange_ReturnsTheFreshSession()
        {
            var dto = new ConfirmEmailChangeDto { NewEmail = "new@test.com" };
            _service.Setup(s => s.ConfirmEmailChangeAsync(8, dto))
                .ReturnsAsync(ProfileResult<AuthResponse>.Ok(new AuthResponse { AccessToken = "fresh" }));

            var result = await _sut.ConfirmEmailChange(dto);

            Assert.That(((AuthResponse)((OkObjectResult)result).Value!).AccessToken, Is.EqualTo("fresh"));
        }

        // ---------- no usable id in the token ----------

        [Test]
        public async Task EveryAction_WithoutAUserIdInTheToken_ReturnsUnauthorized()
        {
            var sut = new ProfileController(_service.Object).WithUser(null);

            Assert.That(await sut.Get(), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.ChangeUsername(new ChangeUsernameDto()), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.ChangePhone(new ChangePhoneDto()), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.ChangePassword(new ChangePasswordDto()), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.StartPasswordChange(), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.ConfirmPasswordChange(new ConfirmPasswordChangeDto()), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.StartEmailChange(new StartEmailChangeDto()), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.ConfirmEmailChange(new ConfirmEmailChangeDto()), Is.InstanceOf<UnauthorizedResult>());
            _service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Get_WithANonNumericId_ReturnsUnauthorized()
        {
            var sut = new ProfileController(_service.Object).WithClaims(new Claim(ClaimTypes.NameIdentifier, "not-a-number"));

            Assert.That(await sut.Get(), Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Get_FallsBackToTheNameidClaim()
        {
            _service.Setup(s => s.GetProfileAsync(21)).ReturnsAsync(OkProfile());
            var sut = new ProfileController(_service.Object).WithClaims(new Claim("nameid", "21"));

            Assert.That(await sut.Get(), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Get_FallsBackToTheSubClaim()
        {
            _service.Setup(s => s.GetProfileAsync(22)).ReturnsAsync(OkProfile());
            var sut = new ProfileController(_service.Object).WithClaims(new Claim("sub", "22"));

            Assert.That(await sut.Get(), Is.InstanceOf<OkObjectResult>());
        }
    }
}
