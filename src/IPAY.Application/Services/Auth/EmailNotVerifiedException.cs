namespace IPAY.Application.Services.Auth
{
    /// <summary>
    /// Thrown by password login when the credentials are right but the email address has not
    /// been verified yet. AuthController turns it into a 403 with code EMAIL_NOT_VERIFIED.
    /// </summary>
    public class EmailNotVerifiedException : Exception
    {
        public EmailNotVerifiedException()
            : base("Please verify your email address first. Check your inbox for the verification link.")
        {
        }
    }
}
