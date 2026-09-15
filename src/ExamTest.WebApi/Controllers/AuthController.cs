using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuth<RegisterDto> _register;
        private readonly ILogin _login;

        public AuthController(IAuth<RegisterDto> register, ILogin login)
        {
            _register = register;
            _login = login;
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
    }
}
