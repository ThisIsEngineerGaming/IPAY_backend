using System;
using System.Collections.Generic;
using System.Text;
using ExamTest.Application.DTOs.Auth;
using FluentValidation;

namespace ExamTest.Application.Validators.Auth
{


    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            // Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен")
                .EmailAddress().WithMessage("Некорректный формат email");

            // Пароль
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен")
                .MinimumLength(8).WithMessage("Пароль должен быть не менее 8 символов");
        }
    }

}
