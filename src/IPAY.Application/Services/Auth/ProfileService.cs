using FluentValidation;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Validators.Auth;
using IPAY.Domain.Entities.Users;

namespace IPAY.Application.Services.Auth
{
    public class ProfileService : IProfileService
    {
        // How long a Firebase sign-in / password reset stays good enough to finish an emailed reset.
        private static readonly TimeSpan SignInFreshness = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan ResetFreshness = TimeSpan.FromMinutes(30);

        private const string NotConfirmed =
            "We could not confirm your new password. Open the link we emailed you, set the new password there, then try again.";

        private readonly IUser<Customer> _customers;
        private readonly IPasswordHashingService _passwordHasher;
        private readonly IFirebaseAuthService _firebase;
        private readonly IJWT _jwt;
        private readonly IValidator<ChangeUsernameDto> _usernameValidator;
        private readonly IValidator<ChangePhoneDto> _phoneValidator;
        private readonly IValidator<ChangePasswordDto> _passwordValidator;
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
            IValidator<ChangePasswordDto> passwordValidator,
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
            _passwordValidator = passwordValidator;
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

        // ---- Password change, option 1: current password + new password ---------------------------

        public async Task<ProfileResult<ProfileDto>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var validation = await _passwordValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<ProfileDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            if (!HasPassword(user))
                return ProfileResult<ProfileDto>.Fail("This account has no password yet. Use \"Create password\" to add one.");

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

        // ---- Password change, option 2: emailed Firebase reset link --------------------------------

        public async Task<ProfileResult<ProfileDto>> StartPasswordChangeAsync(int userId)
        {
            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<ProfileDto>.NotFound();

            // Google-only accounts (no password yet) are allowed here: this is how they create one. The emailed
            // Firebase reset link is the proof of ownership either way.
            if (string.IsNullOrWhiteSpace(user.Email))
                return ProfileResult<ProfileDto>.Fail("Your account has no email address to send the confirmation to.");

            // Older accounts never got a Firebase account, and without one Firebase would silently
            // send nothing. Create it here (with a random password) so the reset email really goes out.
            try
            {
                await _firebase.EnsureAccountAsync(user.Email);
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

            // The token must be a real Firebase sign-in for THIS account...
            var identity = await _firebase.VerifyIdTokenAsync(dto.IdToken);
            if (identity is null || string.IsNullOrWhiteSpace(user.Email) ||
                !string.Equals(identity.Email.Trim(), user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                return ProfileResult<ProfileDto>.Fail(NotConfirmed);

            // ...and the emailed reset link must really have been used. There is no current password to
            // check here, so this proof is the whole defence.
            if (await ResetProofProblemAsync(user.Email, identity) is { } problem)
                return ProfileResult<ProfileDto>.Fail(problem);

            // Firebase already holds the new password (the person set it through the emailed link and just
            // signed in with it), so only our own copy needs to catch up.
            user.Password = _passwordHasher.Hash(dto.NewPassword);
            await _customers.UpdateAsync(userId, user);

            return ProfileResult<ProfileDto>.Ok(ToProfile(user));
        }

        // ---- "Forgot password?" (nobody signed in) --------------------------------------------------

        public async Task StartForgotPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            try
            {
                var user = await _customers.GetByEmail(email.Trim());

                // Unknown, banned and Google-only accounts are skipped silently: the caller always answers
                // the same way, so this endpoint cannot be used to find out who has an account.
                if (user is null || user.IsBanned || !HasPassword(user) || string.IsNullOrWhiteSpace(user.Email))
                    return;

                await _firebase.EnsureAccountAsync(user.Email);
            }
            catch (Exception)
            {
                // Same reason: a failure must look exactly like success to the outside.
            }
        }

        public async Task<ProfileResult<bool>> ResetForgottenPasswordAsync(ConfirmPasswordChangeDto dto)
        {
            var validation = await _confirmPasswordValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<bool>.Fail(validation.Errors[0].ErrorMessage);

            // Nobody is signed in: the account is whoever the verified Firebase token says it is.
            var identity = await _firebase.VerifyIdTokenAsync(dto.IdToken);
            if (identity is null || string.IsNullOrWhiteSpace(identity.Email))
                return ProfileResult<bool>.Fail(NotConfirmed);

            var user = await _customers.GetByEmail(identity.Email.Trim());

            // One message for every "no": it must not reveal whether an account exists.
            if (user is null || user.IsBanned || !HasPassword(user) || string.IsNullOrWhiteSpace(user.Email))
                return ProfileResult<bool>.Fail(NotConfirmed);

            if (await ResetProofProblemAsync(user.Email, identity) is { } problem)
                return ProfileResult<bool>.Fail(problem);

            user.Password = _passwordHasher.Hash(dto.NewPassword);
            await _customers.UpdateAsync(user.Id!.Value, user);

            return ProfileResult<bool>.Ok(true);
        }

        // ---- Email change ---------------------------------------------------------------------------

        // Email changes are confirmed ONLY through the Firebase email sent to the new address - no password.
        public async Task<ProfileResult<StartEmailChangeResultDto>> StartEmailChangeAsync(int userId, StartEmailChangeDto dto)
        {
            var validation = await _startEmailValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ProfileResult<StartEmailChangeResultDto>.Fail(validation.Errors[0].ErrorMessage);

            var user = await GetActiveUserAsync(userId);
            if (user is null)
                return ProfileResult<StartEmailChangeResultDto>.NotFound();

            // Google accounts get their address from Google.
            if (!HasPassword(user))
                return ProfileResult<StartEmailChangeResultDto>.Fail("Your email comes from your Google account and can't be changed here.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return ProfileResult<StartEmailChangeResultDto>.Fail("Your account has no current email address to change.");

            var newEmail = dto.NewEmail.Trim().ToLowerInvariant();

            if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                return ProfileResult<StartEmailChangeResultDto>.Fail("That is already your email address.");

            if (await _customers.GetByEmail(newEmail) is not null)
                return ProfileResult<StartEmailChangeResultDto>.Fail("That email address is already in use.");

            try
            {
                // Older accounts have no Firebase account yet; make sure there is one, mark it as having a change
                // pending for this customer, and hand the client a token to ask Firebase for the email with.
                await _firebase.EnsureAccountAsync(user.Email);
                var token = await _firebase.BeginEmailChangeAsync(user.Email, userId);

                return ProfileResult<StartEmailChangeResultDto>.Ok(new StartEmailChangeResultDto { FirebaseToken = token });
            }
            catch (Exception)
            {
                return ProfileResult<StartEmailChangeResultDto>.Fail(
                    "We could not start the email change right now. Please try again.");
            }
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

            var newEmail = dto.NewEmail.Trim().ToLowerInvariant();

            if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                return ProfileResult<AuthResponse>.Fail("That is already your email address.");

            // Firebase vouches for the new address only once the person has opened the link we sent there, and
            // only for the change THIS account started (the marker set in step 1).
            bool confirmed;
            try
            {
                confirmed = await _firebase.IsEmailChangeConfirmedAsync(newEmail, userId);
            }
            catch (Exception)
            {
                return ProfileResult<AuthResponse>.Fail(
                    "We could not check the confirmation right now. Please try again.");
            }

            if (!confirmed)
                return ProfileResult<AuthResponse>.Fail(
                    "We could not confirm the new email address yet. Open the link we sent to it, then try again.");

            // Checked again here: someone may have taken the address since step 1.
            if (await _customers.GetByEmail(newEmail) is not null)
                return ProfileResult<AuthResponse>.Fail("That email address is already in use.");

            user.Email = newEmail;
            user.EmailVerified = true;
            await _customers.UpdateAsync(userId, user);

            try { await _firebase.CompleteEmailChangeAsync(newEmail); }
            catch (Exception) { /* best effort: the change itself is done */ }

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

        // ---- helpers --------------------------------------------------------------------------------

        private async Task<Customer?> GetActiveUserAsync(int userId)
        {
            var user = await _customers.GetByIdAsync(userId);
            return user is null || user.IsBanned ? null : user;
        }

        /// <summary>
        /// Did the person really go through the emailed Firebase reset link? Returns a message safe to show,
        /// or null when the proof holds. Shared by the signed-in emailed reset and "forgot password".
        /// </summary>
        private async Task<string?> ResetProofProblemAsync(string email, FirebaseIdentity identity)
        {
            // An unverified token proves nothing about the address: anyone can create a Firebase account
            // with somebody else's email. Using the emailed reset link is what makes it verified.
            if (!identity.EmailVerified)
                return NotConfirmed;

            // A recent sign-in, so an old token cannot be replayed.
            var now = DateTimeOffset.UtcNow;
            if (identity.AuthTime is not { } signedInAt || now - signedInAt > SignInFreshness)
                return "This confirmation has expired. Please start again.";

            // Firebase's password must have been changed recently (that is what the emailed link does), and the
            // sign-in must have happened after that change. Otherwise anyone who knows the current password
            // could skip the email by signing in to Firebase with it.
            var changedAt = await _firebase.GetPasswordChangedAtAsync(email);
            if (changedAt is not { } changed ||
                now - changed > ResetFreshness ||
                signedInAt < changed.AddSeconds(-2))
                return NotConfirmed;

            return null;
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
