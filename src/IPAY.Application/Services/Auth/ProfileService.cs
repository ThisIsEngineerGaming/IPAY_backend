using FluentValidation;
using System.Security.Cryptography;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Validators.Auth;
using IPAY.Domain.Entities.Users;

namespace IPAY.Application.Services.Auth
{
    public class ProfileService : IProfileService
    {
        // How long a Firebase sign-in / password reset stays good enough to finish a password change.
        private static readonly TimeSpan SignInFreshness = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan ResetFreshness = TimeSpan.FromMinutes(30);

        private readonly IUser<Customer> _customers;
        private readonly IPasswordHashingService _passwordHasher;
        private readonly IFirebaseAuthService _firebase;
        private readonly IJWT _jwt;
        private readonly IValidator<ChangeUsernameDto> _usernameValidator;
        private readonly IValidator<ChangePhoneDto> _phoneValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IValidator<ConfirmPasswordChangeDto> _confirmPasswordValidator;
        private readonly IValidator<StartEmailChangeDto> _startEmailValidator;
        private readonly IValidator<ConfirmEmailChangeDto> _confirmEmailValidator;

        public ProfileService(
            IUser<Customer> customers,
            IPasswordHashingService passwordHasher,
            IFirebaseAuthService firebase,
            IJWT jwt,
            IValidator<ChangeUsernameDto> usernameValidator,
            IValidator<ChangePhoneDto> phoneValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<ConfirmPasswordChangeDto> confirmPasswordValidator,
            IValidator<StartEmailChangeDto> startEmailValidator,
            IValidator<ConfirmEmailChangeDto> confirmEmailValidator)
        {
            _customers = customers;
            _passwordHasher = passwordHasher;
            _firebase = firebase;
            _jwt = jwt;
            _usernameValidator = usernameValidator;
            _phoneValidator = phoneValidator;
            _changePasswordValidator = changePasswordValidator;
            _confirmPasswordValidator = confirmPasswordValidator;
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

            if (PasswordProblem(user, dto.Password) is { } problem)
                return ProfileResult<ProfileDto>.Fail(problem);

            user.Name = dto.NewName.Trim();
            await _customers.UpdateAsync(userId, user);

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<ProfileDto>> ChangePhoneAsync(int userId, ChangePhoneDto dto)
        {
            var validation = await _phoneValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<ProfileDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            if (PasswordProblem(user, dto.Password) is { } problem)
                return ProfileResult<ProfileDto>.Fail(problem);

            // The validator already accepted it, so this cannot fail - but never store the raw input.
            if (!PhoneRules.TryNormalize(dto.NewPhone, out var phone))
                return ProfileResult<ProfileDto>.Fail("Enter a valid phone number.");

            user.Phone = phone;
            await _customers.UpdateAsync(userId, user);

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<ProfileDto>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var validation = await _changePasswordValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<ProfileDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            if (!HasPassword(user))
                return ProfileResult<ProfileDto>.Fail("This account signs in with Google, so it has no password to change.");

            if (!_passwordHasher.Verify(dto.CurrentPassword, user.Password!))
                return ProfileResult<ProfileDto>.Fail("Current password is incorrect.");

            // Keep the Firebase account's password in step with ours (the email-change flow signs in to
            // Firebase with it). Done first so a Firebase failure leaves nothing half-changed on our side.
            // No Firebase account yet (older user) just returns false: nothing to sync.
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _firebase.SetPasswordAsync(user.Email, dto.NewPassword);
                }
                catch (Exception)
                {
                    return ProfileResult<ProfileDto>.Fail(
                        "We could not update your password right now. Please try again.");
                }
            }

            user.Password = _passwordHasher.Hash(dto.NewPassword);
            await _customers.UpdateAsync(userId, user);

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<ProfileDto>> StartPasswordChangeAsync(int userId)
        {
            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            if (!HasPassword(user))
                return ProfileResult<ProfileDto>.Fail("This account signs in with Google, so it has no password to change.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return ProfileResult<ProfileDto>.Fail("Your account has no email address to send the confirmation to.");

            // Older accounts never got a Firebase account, and without one Firebase would silently
            // send nothing. Create it here (with a random password) so the reset email really goes out.
            //
            // Then lock the Firebase account behind that email: its password becomes a random value nobody
            // knows. Otherwise anyone who knows the current password could sign in to Firebase and change
            // the password with the SDK, skipping the email. From now on the only way in is the reset link.
            try
            {
                await _firebase.EnsureAccountAsync(user.Email);
                await _firebase.SetPasswordAsync(
                    user.Email, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
            }
            catch (Exception)
            {
                return ProfileResult<ProfileDto>.Fail(
                    "We could not prepare the confirmation email right now. Please try again.");
            }

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task<ProfileResult<ProfileDto>> ConfirmPasswordChangeAsync(int userId, ConfirmPasswordChangeDto dto)
        {
            var validation = await _confirmPasswordValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<ProfileDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            if (!HasPassword(user))
                return ProfileResult<ProfileDto>.Fail("This account signs in with Google, so it has no password to change.");

            var identity = await _firebase.VerifyIdTokenAsync(dto.IdToken);
            if (await CheckResetProofAsync(user, identity) is { } proofProblem)
                return ProfileResult<ProfileDto>.Fail(proofProblem);

            // Firebase already holds the new password (the person set it through the emailed link and just
            // signed in with it), so only our own copy needs to catch up.
            user.Password = _passwordHasher.Hash(dto.NewPassword);
            await _customers.UpdateAsync(userId, user);

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        public async Task StartForgotPasswordAsync(string? email)
        {
            var address = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(address))
                return;

            try
            {
                var user = await _customers.GetByEmail(address);
                if (user is null || user.IsBanned || !HasPassword(user) || string.IsNullOrWhiteSpace(user.Email))
                    return;

                // Older accounts never got a Firebase account, and without one Firebase would silently send
                // nothing. Unlike the signed-in flow the Firebase password is left alone: nobody here has
                // proved anything, and the reset only counts once the emailed link has been used.
                await _firebase.EnsureAccountAsync(user.Email);
            }
            catch (Exception)
            {
                // Deliberately silent: an error must not tell a stranger whether the address exists.
            }
        }

        public async Task<ProfileResult<bool>> ResetForgottenPasswordAsync(ConfirmPasswordChangeDto dto)
        {
            var validation = await _confirmPasswordValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<bool>.Fail(validation.Errors[0].ErrorMessage);

            const string notConfirmed =
                "We could not confirm your new password. Open the link we emailed you, set the new password there, then try again.";

            // The account is whoever Firebase says signed in - never an email taken from the request.
            var identity = await _firebase.VerifyIdTokenAsync(dto.IdToken);
            if (identity is null || string.IsNullOrWhiteSpace(identity.Email))
                return ProfileResult<bool>.Fail(notConfirmed);

            var user = await _customers.GetByEmail(identity.Email.Trim().ToLowerInvariant());
            if (user is null || user.IsBanned || user.Id is not int userId)
                return ProfileResult<bool>.Fail(notConfirmed);

            if (!HasPassword(user))
                return ProfileResult<bool>.Fail("This account signs in with Google, so it has no password to reset.");

            if (await CheckResetProofAsync(user, identity) is { } proofProblem)
                return ProfileResult<bool>.Fail(proofProblem);

            // Firebase already holds the new password; only our own copy needs to catch up.
            user.Password = _passwordHasher.Hash(dto.NewPassword);
            await _customers.UpdateAsync(userId, user);

            return ProfileResult<bool>.Ok(true);
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

            // The browser signs in to Firebase with this password to send the link to the new address.
            // Re-sync it now that the password is verified (an abandoned password change leaves a random
            // one behind). No Firebase account yet (older user) just returns false: the browser creates it.
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _firebase.SetPasswordAsync(user.Email, dto.Password);
                }
                catch (Exception)
                {
                    return ProfileResult<ProfileDto>.Fail(
                        "We could not prepare the confirmation email right now. Please try again.");
                }
            }

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

        /// <summary>
        /// The "confirm with your password" rule shared by name and phone changes. Returns a message
        /// safe to show, or null when fine. Accounts created through Google have no password to ask for.
        /// </summary>
        private string? PasswordProblem(Customer user, string password)
        {
            if (!HasPassword(user))
                return null;

            if (string.IsNullOrEmpty(password))
                return "Enter your password to confirm the change.";

            return _passwordHasher.Verify(password, user.Password!) ? null : "Incorrect password.";
        }

        /// <summary>
        /// Did this person really use the emailed Firebase reset link? Returns a message safe to show, or null
        /// when the proof holds. Shared by the signed-in and the "forgot password" flows.
        /// </summary>
        private async Task<string?> CheckResetProofAsync(Customer user, FirebaseIdentity? identity)
        {
            const string notConfirmed =
                "We could not confirm your new password. Open the link we emailed you, set the new password there, then try again.";

            // 1. The token must be a real Firebase sign-in for THIS account.
            if (identity is null || string.IsNullOrWhiteSpace(user.Email) ||
                !string.Equals(identity.Email.Trim(), user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                return notConfirmed;

            // 2. ...a recent one, so an old token cannot be replayed.
            var now = DateTimeOffset.UtcNow;
            if (identity.AuthTime is not { } signedInAt || now - signedInAt > SignInFreshness)
                return "This confirmation has expired. Please start again.";

            // 3. Firebase's password must have been changed recently (that is what the emailed link does),
            //    and the sign-in must have happened after that change. Without this, anyone who knows the
            //    current password could skip the email by signing in to Firebase with it.
            var changedAt = await _firebase.GetPasswordChangedAtAsync(user.Email);
            if (changedAt is not { } changed ||
                now - changed > ResetFreshness ||
                signedInAt < changed.AddSeconds(-2))
                return notConfirmed;

            return null;
        }

        private static bool HasPassword(Customer user) => !string.IsNullOrEmpty(user.Password);

        private static ProfileDto ToProfile(Customer user) => new()
        {
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            HasPassword = HasPassword(user)
        };
    }
}
