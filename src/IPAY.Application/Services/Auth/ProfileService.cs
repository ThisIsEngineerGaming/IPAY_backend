using FluentValidation;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;

namespace IPAY.Application.Services.Auth
{
    public class ProfileService : IProfileService
    {
        private readonly IUser<Customer> _customers;
        private readonly IPasswordHashingService _passwordHasher;
        private readonly IFirebaseAuthService _firebase;
        private readonly IJWT _jwt;
        private readonly IValidator<ChangeUsernameDto> _usernameValidator;
        private readonly IValidator<ChangePasswordDto> _passwordValidator;
        private readonly IValidator<StartEmailChangeDto> _startEmailValidator;
        private readonly IValidator<ConfirmEmailChangeDto> _confirmEmailValidator;

        public ProfileService(
            IUser<Customer> customers,
            IPasswordHashingService passwordHasher,
            IFirebaseAuthService firebase,
            IJWT jwt,
            IValidator<ChangeUsernameDto> usernameValidator,
            IValidator<ChangePasswordDto> passwordValidator,
            IValidator<StartEmailChangeDto> startEmailValidator,
            IValidator<ConfirmEmailChangeDto> confirmEmailValidator)
        {
            _customers = customers;
            _passwordHasher = passwordHasher;
            _firebase = firebase;
            _jwt = jwt;
            _usernameValidator = usernameValidator;
            _passwordValidator = passwordValidator;
            _startEmailValidator = startEmailValidator;
            _confirmEmailValidator = confirmEmailValidator;
        }

        public async Task<ProfileResult<ProfileDto>> GetProfileAsync(int userId)
        {
            var user = await GetActiveUserAsync(userId);
            return user is null
                ? ProfileResult<ProfileDto>.NotFound()
                : ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<ProfileDto>> ChangeUsernameAsync(int userId, ChangeUsernameDto dto)
        {
            var validation = await _usernameValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<ProfileDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            // Accounts created through Google have no password, so there is nothing to confirm with.
            if (HasPassword(user))
            {
                if (string.IsNullOrEmpty(dto.Password))
                    return ProfileResult<ProfileDto>.Fail("Enter your password to confirm the change.");

                if (!_passwordHasher.Verify(dto.Password, user.Password!))
                    return ProfileResult<ProfileDto>.Fail("Incorrect password.");
            }

            user.Name = dto.NewName.Trim();
            await _customers.UpdateAsync(userId, user);

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<ProfileDto>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var validation = await _passwordValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<ProfileDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            if (!HasPassword(user))
                return ProfileResult<ProfileDto>.Fail("This account signs in with Google, so it has no password to change.");

            if (!_passwordHasher.Verify(dto.CurrentPassword, user.Password!))
                return ProfileResult<ProfileDto>.Fail("Current password is incorrect.");

            var email = user.Email;

            // Firebase first: if it fails nothing has changed anywhere. (No Firebase account for this
            // email - an older account - is fine: SetPasswordAsync just returns false.)
            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    await _firebase.SetPasswordAsync(email, dto.NewPassword);
                }
                catch (Exception)
                {
                    return ProfileResult<ProfileDto>.Fail("We could not update your password right now. Please try again.");
                }
            }

            try
            {
                user.Password = _passwordHasher.Hash(dto.NewPassword);
                await _customers.UpdateAsync(userId, user);
            }
            catch (Exception)
            {
                // Saving failed after Firebase already changed: put Firebase back so both stay in sync.
                if (!string.IsNullOrWhiteSpace(email))
                {
                    try { await _firebase.SetPasswordAsync(email, dto.CurrentPassword); }
                    catch (Exception) { /* best effort */ }
                }
                throw;
            }

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<ProfileDto>> StartEmailChangeAsync(int userId, StartEmailChangeDto dto)
        {
            var validation = await _startEmailValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<ProfileDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            if (!HasPassword(user))
                return ProfileResult<ProfileDto>.Fail("Your email comes from your Google account and can't be changed here.");

            if (!_passwordHasher.Verify(dto.Password, user.Password!))
                return ProfileResult<ProfileDto>.Fail("Incorrect password.");

            var newEmail = dto.NewEmail.Trim().ToLowerInvariant();

            if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                return ProfileResult<ProfileDto>.Fail("That is already your email address.");

            if (await _customers.GetByEmail(newEmail) is not null)
                return ProfileResult<ProfileDto>.Fail("That email address is already in use.");

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<AuthResponse>> ConfirmEmailChangeAsync(int userId, ConfirmEmailChangeDto dto)
        {
            var validation = await _confirmEmailValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<AuthResponse>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<AuthResponse>.NotFound();

            if (!HasPassword(user))
                return ProfileResult<AuthResponse>.Fail("Your email comes from your Google account and can't be changed here.");

            if (!_passwordHasher.Verify(dto.Password, user.Password!))
                return ProfileResult<AuthResponse>.Fail("Incorrect password.");

            // Firebase vouches for the new address only once the person has clicked the link we sent there.
            var identity = await _firebase.VerifyIdTokenAsync(dto.IdToken);
            if (identity is null || string.IsNullOrWhiteSpace(identity.Email) || !identity.EmailVerified)
                return ProfileResult<AuthResponse>.Fail(
                    "We could not confirm the new email address. Open the link we sent to it, then try again.");

            var newEmail = identity.Email.Trim().ToLowerInvariant();

            if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                return ProfileResult<AuthResponse>.Fail("That is already your email address.");

            // Checked again here: someone may have taken the address since step 1.
            if (await _customers.GetByEmail(newEmail) is not null)
                return ProfileResult<AuthResponse>.Fail("That email address is already in use.");

            user.Email = newEmail;
            user.EmailVerified = true;
            await _customers.UpdateAsync(userId, user);

            // The JWT carries the email claim, so the old token is stale: hand back a fresh session.
            var token = _jwt.GenerateToken(userId.ToString(), newEmail, user.Role);

            return ProfileResult<AuthResponse>.Ok(new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                User = new LoginDto { Email = newEmail, Name = user.Name }
            });
        }

        private async Task<Customer?> GetActiveUserAsync(int userId)
        {
            var user = await _customers.GetByIdAsync(userId);
            return user is null || user.IsBanned ? null : user;
        }

        private static bool HasPassword(Customer user) => !string.IsNullOrEmpty(user.Password);

        private static ProfileDto ToProfile(Customer user) => new()
        {
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            HasPassword = HasPassword(user)
        };
    }
}
