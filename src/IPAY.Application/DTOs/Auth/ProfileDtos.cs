namespace IPAY.Application.DTOs.Auth
{
    /// <summary>What the profile page shows about the signed-in user.</summary>
    public class ProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>Empty when no number was added yet.</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// False for accounts created through Google sign-in (they have no password to confirm with):
        /// those can rename themselves and change their phone, but their email and password are managed by Google.
        /// </summary>
        public bool HasPassword { get; set; }
    }

    public class ChangeUsernameDto
    {
        public string NewName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePhoneDto
    {
        public string NewPhone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Password change, option 1: type the current password and the new one. No email step.</summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Step 2 of an emailed password reset, after the person opened the Firebase reset email and chose the new
    /// password there. IdToken is a Firebase ID token from signing in to Firebase with that new password.
    /// No current password is involved: the emailed link is the proof. Used both by the signed-in profile page
    /// (api/profile/password/confirm) and by "forgot password" (api/auth/password-reset/confirm).
    /// </summary>
    public class ConfirmPasswordChangeDto
    {
        public string IdToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>"Forgot password?" step 1: the address to send the Firebase reset link to.</summary>
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>Step 1 of an email change: check the password and that the new address is free.</summary>
    public class StartEmailChangeDto
    {
        public string NewEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Step 2 of an email change, after the person clicked the Firebase link sent to the NEW address.
    /// IdToken is a Firebase ID token for the account that now carries the (verified) new email.
    /// </summary>
    public class ConfirmEmailChangeDto
    {
        public string IdToken { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
