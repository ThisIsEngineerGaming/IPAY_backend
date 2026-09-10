using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers
{
    [ApiController]
    [Route("api/register")]
    public class AuthController:ControllerBase
    {

        private readonly IAuth<RegisterDto> _register;
        private readonly IAuth<LoginDto> _login;

        public AuthController(IAuth<RegisterDto> register, IAuth<LoginDto> login) 
        {
            _register = register;
            _login = login;
        }
        [HttpPost("register")]
        public async Task<ActionResult<RegisterDto>> Registration(RegisterDto register)
        {
            if (!await _register.Register(register))
            {
                return BadRequest("Неправильно введені дані!");
            }
            else
            {
                return Ok("Реєстрація пройшла успішно!");
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<LoginDto>> Loginn(LoginDto login)
        {
            if (!await _login.Register(login))
            {
                return BadRequest("Неправильно введені дані!");
            }
            else
            {
                return Ok("Реєстрація пройшла успішно!");
            }
        }



    }
}
