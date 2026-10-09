using System.Security.Claims;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers
{
    /// <summary>The signed-in user's own profile. Every action works on the account in the JWT.</summary>
    [ApiController]
    [Authorize]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profile;

        public ProfileController(IProfileService profile)
        {
            _profile = profile;
        }

        // -> { name, email, phone, hasPassword }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.GetProfileAsync(id));
        }

        // Body: { newName, password } -> { name, email, phone, hasPassword }
        [HttpPut("username")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto)
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.ChangeUsernameAsync(id, dto));
        }

        // Body: { newPhone, password } -> { name, email, phone, hasPassword }
        [HttpPut("phone")]
        public async Task<IActionResult> ChangePhone([FromBody] ChangePhoneDto dto)
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.ChangePhoneAsync(id, dto));
        }

        // Password change, option 1: current password + new one.
        // Body: { currentPassword, newPassword } -> { name, email, phone, hasPassword }
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.ChangePasswordAsync(id, dto));
        }

        // Password change, option 2 (email reset), step 1. No body -> { name, email, phone, hasPassword }
        // The client then asks Firebase to email the reset link.
        [HttpPost("password/start")]
        public async Task<IActionResult> StartPasswordChange()
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.StartPasswordChangeAsync(id));
        }

        // Password change, option 2 (email reset), step 2.
        // Body: { idToken, newPassword } -> { name, email, phone, hasPassword }
        [HttpPost("password/confirm")]
        public async Task<IActionResult> ConfirmPasswordChange([FromBody] ConfirmPasswordChangeDto dto)
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.ConfirmPasswordChangeAsync(id, dto));
        }

        // Email change, step 1. Body: { newEmail, password } -> { name, email, phone, hasPassword }
        [HttpPost("email/start")]
        public async Task<IActionResult> StartEmailChange([FromBody] StartEmailChangeDto dto)
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.StartEmailChangeAsync(id, dto));
        }

        // Email change, step 2. Body: { idToken, password } -> AuthResponse (fresh JWT carrying the new email)
        [HttpPost("email/confirm")]
        public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeDto dto)
        {
            if (CurrentUserId() is not int id) return Unauthorized();
            return ToResponse(await _profile.ConfirmEmailChangeAsync(id, dto));
        }

        private int? CurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("nameid")
                      ?? User.FindFirstValue("sub");

            return int.TryParse(raw, out var id) ? id : null;
        }

        // 400 (not 401) for "wrong password" and friends: the frontend's axios interceptor
        // hard-redirects to /login on any 401.
        private IActionResult ToResponse<T>(ProfileResult<T> result)
        {
            if (result.Succeeded)
                return Ok(result.Value);

            return result.Failure == ProfileFailure.NotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }
    }
}
