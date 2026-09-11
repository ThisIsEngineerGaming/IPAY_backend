using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using ExamTest.Domain.Interfaces.ForRepos;
using FluentValidation;
using System.Threading.Tasks;

namespace ExamTest.Application.Services.Auth
{
    public class RegisterService : IAuth<RegisterDto>
    {
        private readonly IValidator<RegisterDto> _validator;

        public RegisterService(IValidator<RegisterDto> validator)
        {
            _validator = validator;
        }

        public async Task<bool> Register(RegisterDto registerDto)
        {
            var result = await _validator.ValidateAsync(registerDto);
            return result.IsValid;
        }
    }
}

