namespace IPAY.Application.DTOs.Auth
{
    /// <summary>What the profile page shows about the signed-in user.</summary>
    public class ProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>False for accounts created through Google sign-in (they have no password to confirm with).</summary>
        public bool HasPassword { get; set; }
    }

    public class ChangeUsernameDto
    {
        public string NewName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>Step 1 of an email change: check the password and that the new address is free.</summary>
    public class StartEmailChangeDto
    {
        public string NewEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Step 2 of an email change, after the person clicked the Firebase link sent to the new address.
    /// IdToken is a Firebase ID token for the account that now carries the (verified) new email.
    /// </summary>
    public class ConfirmEmailChangeDto
    {
        public string IdToken { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
