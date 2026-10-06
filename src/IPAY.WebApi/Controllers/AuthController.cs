using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Permissions;

namespace IPAY.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthDto<RegisterDto> _register;
        private readonly ILogin _login;
        private readonly IGoogleLogin _googleLogin;
        private readonly ITwoFactorService _twoFactor;

        private readonly IUser<Customer> _customer;

        public AuthController(IAuthDto<RegisterDto> register, ILogin login, IUser<Customer> customer, IGoogleLogin googleLogin, ITwoFactorService twoFactor)
        {
            _register = register;
            _login = login;
            _customer = customer;
            _googleLogin = googleLogin;
            _twoFactor = twoFactor;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registration([FromBody] RegisterDto register)
        {
            if (!await _register.Register(register))
            {
                return BadRequest("Неправильно введені дані!");
            }

            return Ok("Реєстрація пройшла успішно!");
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto login)
        {
            AuthResponse? result;
            try
            {
                result = await _login.LoginAsync(login);
            }
            catch (EmailNotVerifiedException ex)
            {
                // Frontend keys off `code` to offer "resend verification email".
                return StatusCode(StatusCodes.Status403Forbidden, new { code = "EMAIL_NOT_VERIFIED", message = ex.Message });
            }
            catch (TwoFactorDeliveryException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
            }

            if (result == null)
            {
                return BadRequest("Неверный email или пароль");
            }

            return Ok(result);
        }

        // Second step of a password login when emailed codes are on.
        // Body: { challengeId, code } -> the real AuthResponse (JWT).
        [HttpPost("login/verify-code")]
        public async Task<ActionResult<AuthResponse>> VerifyCode([FromBody] VerifyCodeDto dto)
        {
            var result = await _twoFactor.VerifyAsync(dto.ChallengeId, dto.Code);

            // 400 (not 401): the frontend's axios interceptor hard-redirects to /login on any 401.
            if (!result.Succeeded)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        // Body: { challengeId } -> emails a fresh code (30s cooldown).
        [HttpPost("login/resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] ResendCodeDto dto)
        {
            try
            {
                var result = await _twoFactor.ResendAsync(dto.ChallengeId);
                if (!result.Succeeded)
                    return BadRequest(result.Error);

                return Ok(new { maskedEmail = result.Value });
            }
            catch (TwoFactorDeliveryException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
            }
        }

        // Body: { idToken } - the Firebase ID token the client got from "Sign in with Google".
        [HttpPost("google")]
        public async Task<ActionResult<AuthResponse>> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var result = await _googleLogin.LoginWithGoogleAsync(dto.IdToken);

            if (result == null)
            {
                // 400 (not 401): the frontend's axios interceptor hard-redirects to /login on any 401.
                return BadRequest("Google sign-in failed. Please try again.");
            }

            return Ok(result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("users/{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromQuery] UserRole role)
        {
            var user = await _customer.GetByIdAsync(id); 
            if (user is null)
                return NotFound();

            user.Role = role;
            await _customer.UpdateAsync(id,user); // сохранить в БД/Firestore

            return Ok(user);
        }

        [Authorize(Policy = "ModeratorOnly")]
        [HttpPatch("users/moderator/{id}/role")]
        public async Task<IActionResult> ChangeRoleByModerator(int id, [FromQuery] UserRole role)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
                return NotFound();
            if(role != UserRole.Moderator && role != UserRole.Admin)
            {
                user.Role = role;
                await _customer.UpdateAsync(id, user); // сохранить в БД/Firestore

                return Ok(user);
            }
            else
            {
                return BadRequest("Ти не маешь права!");
            }

        }


        [Authorize(Policy = "CustomerOnly")]
        [HttpPost("seller-request")]
        public async Task<IActionResult> MakeRequest()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var id = int.Parse(userId);
            var user = await _customer.GetByIdAsync(id);
            if (user is null) return NotFound();

            user.Role = UserRole.Seller;
            await _customer.UpdateAsync(id, user);

            return Ok("Succed");
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("admin/{id}/ban")]
        public async Task<IActionResult> BannedByAdmin(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin)
            {
                return Forbid();
            }
            user.IsBanned = true;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);
            

        }

        [Authorize(Policy = "ModeratorOnly")]
        [HttpPatch("moderator/{id}/ban")]
        public async Task<IActionResult> BannedByModerator(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin || user.Role == UserRole.Moderator)
            {
                return Forbid();
            }
            user.IsBanned = true;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);


        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("admin/{id}/unban")]
        public async Task<IActionResult> UnBannedByAdmin(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin)
            {
                return Forbid();
            }
            user.IsBanned = false;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);


        }

        [Authorize(Policy = "ModeratorOnly")]
        [HttpPatch("moderator/{id}/unban")]
        public async Task<IActionResult> UnBannedByModerator(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin || user.Role == UserRole.Moderator)
            {
                return Forbid();
            }
            user.IsBanned = false;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);


        }


    }
}
