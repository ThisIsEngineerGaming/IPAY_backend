using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers
{
    /// <summary>
    /// "Forgot password?" on the login page. Nobody is signed in here, so there is no JWT: the emailed
    /// Firebase reset link is the proof of who the person is.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/auth/password-reset")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IProfileService _profile;

        public PasswordResetController(IProfileService profile)
        {
            _profile = profile;
        }

        // Step 1. Body: { email } -> always 204, whether or not the address belongs to an account.
        // The client then asks Firebase to email the reset link.
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] ForgotPasswordDto dto)
        {
            await _profile.StartForgotPasswordAsync(dto.Email);
            return NoContent();
        }

        // Step 2. Body: { idToken, newPassword } -> 204.
        // idToken is a Firebase ID token from signing in with the new password set through the link.
        // 400 (not 401) on failure: the frontend's axios interceptor hard-redirects to /login on any 401.
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmPasswordChangeDto dto)
        {
            var result = await _profile.ResetForgottenPasswordAsync(dto);
            return result.Succeeded ? NoContent() : BadRequest(result.Error);
        }
    }
}
