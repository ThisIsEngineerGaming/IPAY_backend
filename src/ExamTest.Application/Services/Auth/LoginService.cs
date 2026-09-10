using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.Services.Auth
{
   public  class LoginService: IAuth<LoginDto>
    {
        private readonly IValidator<LoginDto> _validator;

        public LoginService(IValidator<LoginDto> validator)
        {
            _validator = validator;
        }
        public async Task<bool> Register(LoginDto loginDto)
        {
            var result = await _validator.ValidateAsync(loginDto);
            return result.IsValid;
        }
    }
}
