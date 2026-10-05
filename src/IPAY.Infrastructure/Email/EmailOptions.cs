namespace IPAY.Infrastructure.Email
{
    /// <summary>
    /// Bound from the "Email" section of appsettings - any SMTP server works (Gmail, Outlook, Brevo,
    /// Mailgun, SendGrid SMTP relay...). Keep the password out of source control (user-secrets or an
    /// environment variable: Email__Password).
    /// </summary>
    public class EmailOptions
    {
        public const string SectionName = "Email";

        /// <summary>SMTP server, e.g. smtp.gmail.com. Empty = email sending is not configured.</summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>587 (STARTTLS, most common) or 465 (implicit SSL).</summary>
        public int Port { get; set; } = 587;

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>The "From" address. Most providers require it to be the account you log in with.</summary>
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "IPAY";
    }
}
