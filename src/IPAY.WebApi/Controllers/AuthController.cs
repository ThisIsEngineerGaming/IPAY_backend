using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
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

        private readonly IUser<Customer> _customer;

        public AuthController(IAuthDto<RegisterDto> register, ILogin login, IUser<Customer> customer)
        {
            _register = register;
            _login = login;
            _customer = customer;
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
            var result = await _login.LoginAsync(login);

            if (result == null)
            {
                return BadRequest("Неверный email или пароль");
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
    }
}
