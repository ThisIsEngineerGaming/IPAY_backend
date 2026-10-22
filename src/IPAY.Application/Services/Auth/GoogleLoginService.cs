using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;

namespace IPAY.Application.Services.Auth
{
    //google registry
    public class GoogleLoginService : IGoogleLogin
    {
        private readonly IFirebaseAuthService _firebase;//firebase gives opportunity for verifying email, that's why we using it
        private readonly IUser<Customer> _customers;//took over a basic user interface
     

        private readonly IRefreshTokenService _refreshTokenService;//refreshing JWT (for example password)

        public GoogleLoginService(IFirebaseAuthService firebase, IUser<Customer> customers,  IRefreshTokenService refreshTokenService)
        {
            _firebase = firebase;
            _customers = customers;
   
            _refreshTokenService = refreshTokenService;
        }
        //method let us to login by google account
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

            return await _refreshTokenService.CreateAuthResponseAsync(
                user.Id.Value,
                user.Email!,
                user.Name,
                user.Role);
        }
    }
}
