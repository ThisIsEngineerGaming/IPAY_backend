using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;

namespace IPAY.Application.Services.Auth
{
    public class GoogleLoginService : IGoogleLogin
    {
        private readonly IFirebaseAuthService _firebase;
        private readonly IUser<Customer> _customers;
        private readonly IJWT _jwt;

        public GoogleLoginService(IFirebaseAuthService firebase, IUser<Customer> customers, IJWT jwt)
        {
            _firebase = firebase;
            _customers = customers;
            _jwt = jwt;
        }

        public async Task<AuthResponse?> LoginWithGoogleAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
                return null;

            var identity = await _firebase.VerifyIdTokenAsync(idToken);

            // Google only vouches for the address when it is verified - never trust an unverified one,
            // otherwise someone could claim another person's email.
            if (identity is null || string.IsNullOrWhiteSpace(identity.Email) || !identity.EmailVerified)
                return null;

            var email = identity.Email.Trim().ToLowerInvariant();

            var user = await _customers.GetByEmail(email);

            if (user is null)
            {
                // First Google sign-in: create a Customer with NO password, so the account can only
                // be entered through Google (password login rejects an empty hash).
                var name = !string.IsNullOrWhiteSpace(identity.Name)
                    ? identity.Name.Trim()
                    : email.Split('@')[0];

                await _customers.CreateAsync(new Customer
                {
                    Email = email,
                    Name = name,
                    Password = string.Empty,
                    Role = UserRole.Customer,
                    EmailVerified = true // Google already verified this address
                });

                user = await _customers.GetByEmail(email);
            }

            else if (user.EmailVerified != true && user.Id is not null)
            {
                // Existing password account whose owner just proved the address through Google.
                user.EmailVerified = true;
                await _customers.UpdateAsync(user.Id.Value, user);
            }


            if (user is null || user.Id is null || string.IsNullOrWhiteSpace(user.Email) || user.IsBanned)
                return null;

            var token = _jwt.GenerateToken(user.Id.Value.ToString(), user.Email, user.Role);

            return new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                User = new LoginDto { Email = user.Email, Name = user.Name }
            };
        }
    }
}
