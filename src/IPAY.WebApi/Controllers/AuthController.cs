using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Auth;
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
        private readonly ILogger<AuthController>? _logger;


        private readonly IRefreshTokenService _refreshTokenService;

        private readonly IGuestService _guest;



        public AuthController(IAuthDto<RegisterDto> register, ILogin login, IUser<Customer> customer, IGuestService guest, IRefreshTokenService refreshTokenService, IGoogleLogin googleLogin, ITwoFactorService twoFactor, ILogger<AuthController>? logger = null)

        {
            _logger = logger;
            _register = register;
            _login = login;
            _customer = customer;
            _googleLogin = googleLogin;
            _twoFactor = twoFactor;
            _refreshTokenService = refreshTokenService;
            _guest = guest;

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
            AuthResponse? result;
            try
            {
                result = await _googleLogin.LoginWithGoogleAsync(dto.IdToken);
            }
            catch (Exception ex)
            {
                // Firestore/network/credential problems used to surface as a bare 500 with no detail.
                _logger?.LogError(ex, "Google sign-in crashed after the token check (user lookup/creation or JWT step).");
                return StatusCode(StatusCodes.Status500InternalServerError, "Google sign-in failed. Please try again.");
            }

            if (result == null)
            {
                _logger?.LogWarning(
                    "Google sign-in returned no result (idToken empty: {Empty}). Look for a preceding 'Google sign-in:' " +
                    "warning from FirebaseAuthService; otherwise the account was banned or could not be created/read.",
                    string.IsNullOrWhiteSpace(dto.IdToken));

                // 400 (not 401): the frontend's axios interceptor hard-redirects to /login on any 401.
                return BadRequest("Google sign-in failed. Please try again.");
            }

            return Ok(result);
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


        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _refreshTokenService.RefreshAsync(request.RefreshToken, ip);

            // 400, не 401 — у вас axios на 401 кидает на /login
            if (result is null)
                return BadRequest("Invalid or expired refresh token");

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await _refreshTokenService.RevokeAsync(request.RefreshToken);
            return Ok();
        }

        [HttpPost("guests/session")]
        public async Task<IActionResult> EnsureGuestSession([FromBody] GuestSessionDto? dto)
        {
            var guest = await _guest.GetOrCreateAsync(dto?.SessionKey, dto?.Name);

            if (guest.IsBanned)
                return StatusCode(403, "Заблоковано");

            var response = await _refreshTokenService.CreateAuthResponseAsync(
                guest.Id,
                $"guest-{guest.Id}@guest.local",
                guest.Name,
                UserRole.Guest,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new
            {
                guest.Id,
                guest.Name,
                guest.Role,
                guest.IsBanned,
                guest.SessionKey,
                guest.CreatedAt,
                accessToken = response.AccessToken,
                refreshToken = response.RefreshToken,
                tokenType = response.TokenType,
                expiresAt = response.ExpiresAt
            });
        }

    }
}
