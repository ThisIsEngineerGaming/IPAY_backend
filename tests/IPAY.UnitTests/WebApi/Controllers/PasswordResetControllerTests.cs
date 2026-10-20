using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class PasswordResetControllerTests
    {
        private Mock<IProfileService> _profile = null!;
        private PasswordResetController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _profile = new Mock<IProfileService>();
            _sut = new PasswordResetController(_profile.Object);
        }

        [Test]
        public async Task Start_AlwaysReturns204SoItCannotRevealWhoHasAnAccount()
        {
            var result = await _sut.Start(new ForgotPasswordDto { Email = "nobody@test.com" });

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _profile.Verify(p => p.StartForgotPasswordAsync("nobody@test.com"), Times.Once);
        }

        [Test]
        public async Task Confirm_OnSuccess_Returns204()
        {
            var dto = new ConfirmPasswordChangeDto();
            _profile.Setup(p => p.ResetForgottenPasswordAsync(dto)).ReturnsAsync(ProfileResult<bool>.Ok(true));

            Assert.That(await _sut.Confirm(dto), Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Confirm_OnFailure_Returns400NotUnauthorised()
        {
            var dto = new ConfirmPasswordChangeDto();
            _profile.Setup(p => p.ResetForgottenPasswordAsync(dto)).ReturnsAsync(ProfileResult<bool>.Fail("nope"));

            var result = await _sut.Confirm(dto);

            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("nope"));
        }
    }
}
