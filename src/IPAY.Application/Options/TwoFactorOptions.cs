namespace IPAY.Application.Options
{
    /// <summary>Bound from the "TwoFactor" section of appsettings.</summary>
    public class TwoFactorOptions
    {
        public const string SectionName = "TwoFactor";

        /// <summary>
        /// Ask for an emailed 6-digit code after the password. Only takes effect when the "Email"
        /// section is configured too (otherwise nobody could ever receive a code).
        /// Google sign-in is not affected - Google has its own account protection.
        /// </summary>
        public bool Enabled { get; set; } = true;

        public int CodeLifetimeMinutes { get; set; } = 10;

        /// <summary>Wrong guesses allowed per code before it is thrown away and the person must sign in again.</summary>
        public int MaxAttempts { get; set; } = 5;

        public int ResendCooldownSeconds { get; set; } = 30;
    }
}
