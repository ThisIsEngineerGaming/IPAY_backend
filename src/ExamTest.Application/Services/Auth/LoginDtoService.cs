using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using ExamTest.Domain.Entities.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;


namespace ExamTest.Application.Services.Auth
{
   public  class LoginDtoService: ILogin
    {
        private readonly IValidator<LoginDto> _validator;

        private readonly IUser<Seller> _seller;

        private readonly IJWT _jwt;

        public LoginDtoService(IValidator<LoginDto> validator,IJWT jwt,IUser<Seller> seller)
        {
            _validator = validator;
            _jwt = jwt;
            _seller= seller;

        }
        public async Task<bool> Login(LoginDto loginDto)
        {
            var result = await _validator.ValidateAsync(loginDto);
            return result.IsValid;
        }
        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            var result = await Login(loginDto);

            if (!result)
            {
                return null;
            }
            else
            {
                // 2. Найти пользователя в базе по Email
                var user = await _seller.GetByEmail(loginDto.Email);

                if (user == null)
                    return null; // пользователь не найден

                // 3. Проверить пароль
                // (пока просто для примера)
                if (user.Password != loginDto.Password)  // потом будет хеш!
                    return null;

                // 4. Теперь у тебя есть user.Id → передаёшь его в JWT
                var token = _jwt.GenerateToken(user.Id.ToString(), user.Email);

                // 5. Возвращаешь AuthResponse
                return new AuthResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresAt = DateTime.UtcNow.AddHours(2),
                    User=loginDto
                };
            }
        }
    }
}
