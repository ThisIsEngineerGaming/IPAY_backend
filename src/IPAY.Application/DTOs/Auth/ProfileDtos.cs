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

    /// <summary>Option 1 of a password change: type the current password and the new one.</summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Option 2 of a password change (email reset), step 2: the person opened the Firebase reset email and chose
    /// the new password there. IdToken is a Firebase ID token from signing in to Firebase with that new password.
    /// The current password is not needed: access to the account's mailbox is the proof.
    /// </summary>
    public class ConfirmPasswordChangeDto
    {
        public string IdToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>"Forgot password?" on the login page, step 1: which account's reset email to prepare.</summary>
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
