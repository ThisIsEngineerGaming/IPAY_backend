using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;


namespace IPAY.Application.Services.Auth
{
   public  class LoginDtoService: ILogin
    {
        private readonly IValidator<LoginDto> _validator;

        private readonly IUser<Customer> _customer;

        private readonly IJWT _jwt;
        private readonly IPasswordHashingService _passwordHasher;
        // Optional so the service can still be built without them (unit tests); the DI container always supplies them.
        private readonly IFirebaseAuthService? _firebase;
        private readonly ITwoFactorService? _twoFactor;

        public LoginDtoService(
            IValidator<LoginDto> validator,
            IJWT jwt,
            IUser<Customer> customer,
            IPasswordHashingService passwordHasher,
            IFirebaseAuthService? firebase = null,
            ITwoFactorService? twoFactor = null)
        {
            _validator = validator;
            _jwt = jwt;
            _customer= customer;
            _passwordHasher = passwordHasher;
            _firebase = firebase;
            _twoFactor = twoFactor;

        }
        public async Task<bool> Login(LoginDto loginDto)
        {
            var result = await _validator.ValidateAsync(loginDto);
            return result.IsValid;
        }
        public async Task<AuthResponse?> LoginAsync(LoginDto loginDto)
        {
            var result = await Login(loginDto);

            if (!result)
            {
                return null;
            }
            else
            {
                // 2. Найти пользователя в базе по Email
                var user = await _customer.GetByEmail(loginDto.Email);

                if (user == null)
                    return null; // пользователь не найден

                if (user.Id is null || string.IsNullOrWhiteSpace(user.Email))
                    return null;

                // 3. Проверить пароль
                // (пока просто для примера)
                if (string.IsNullOrEmpty(user.Password) ||
                    !_passwordHasher.Verify(loginDto.Password, user.Password))
                    return null;


                // 3b. Password is right - now make sure the email address was verified through Firebase.
                // Checked AFTER the password so nobody can probe which emails are registered/unverified.
                if (_firebase is { RequireEmailVerification: true } &&
                    !await _firebase.IsEmailVerifiedAsync(user.Email))
                {
                    throw new EmailNotVerifiedException();
                }

                // 3c. Password (and email) are fine - ask for the emailed code before handing out a session.
                if (_twoFactor is { IsEnabled: true })

                // 3b. Email confirmation is a ONE-TIME step stored on the account (Customer.EmailVerified).
                // Only accounts still marked pending (false) are checked against Firebase; once confirmed
                // it is saved and never asked again. Older accounts (null) are never blocked.
                // Checked AFTER the password so nobody can probe which emails are registered/unverified.
                if (_firebase is { RequireEmailVerification: true } && user.EmailVerified == false)
                {
                    if (!await _firebase.IsEmailVerifiedAsync(user.Email))
                        throw new EmailNotVerifiedException();

                    user.EmailVerified = true;
                    await _customer.UpdateAsync(user.Id.Value, user);
                }

                // 3c. Emailed sign-in code - only for accounts with a confirmed email address.
                // (Older accounts may have placeholder emails, so a code could never reach them.)
                if (_twoFactor is { IsEnabled: true } && user.EmailVerified == true)

                {
                    return await _twoFactor.BeginAsync(user);
                }

                // 4. Теперь у тебя есть user.Id → передаёшь его в JWT
                var token = _jwt.GenerateToken(user.Id.Value.ToString(), user.Email,user.Role);

                // 5. Возвращаешь AuthResponse
                return new AuthResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresAt = DateTime.UtcNow.AddHours(2),
                    User = new LoginDto { Email = user.Email ?? string.Empty, Name = user.Name }
                };
            }
        }
    }
}
