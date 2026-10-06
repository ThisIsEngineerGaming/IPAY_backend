namespace IPAY.Application.Interfaces.Auth
{
    /// <summary>Sends one email. Implemented with SMTP in Infrastructure/Email.</summary>
    public interface IEmailSender
    {
        /// <summary>False when no SMTP server has been configured (the "Email" section of appsettings).</summary>
        bool IsConfigured { get; }

        Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
