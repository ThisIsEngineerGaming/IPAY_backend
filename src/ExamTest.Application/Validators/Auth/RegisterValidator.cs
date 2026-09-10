using System;
using System.Collections.Generic;
using System.Text;
using ExamTest.Application.DTOs.Auth;
using FluentValidation;

namespace ExamTest.Application.Validators.Auth
{

    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            // Имя
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя обязательно")
                .Length(2, 50).WithMessage("Имя должно быть от 2 до 50 символов")
                .Matches(@"^[а-яА-Яa-zA-Z\s\-]+$").WithMessage("Имя может содержать только буквы, пробелы и дефис");

            // Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен")
                .EmailAddress().WithMessage("Некорректный формат email")
                .MaximumLength(100).WithMessage("Email не должен превышать 100 символов");

            // Пароль
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен")
                .MinimumLength(8).WithMessage("Пароль должен быть не менее 8 символов")
                .Matches("[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
                .Matches("[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
                .Matches("[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
                .Matches("[^a-zA-Z0-9]").WithMessage("Пароль должен содержать хотя бы один спецсимвол");

            // Подтверждение пароля
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Подтверждение пароля обязательно")
                .Equal(x => x.Password).WithMessage("Пароли не совпадают");
        }
    }

}
