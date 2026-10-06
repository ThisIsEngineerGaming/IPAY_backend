using IPAY.Application.Interfaces.Auth;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IPAY.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;

        public SmtpEmailSender(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_options.Host) &&
            !string.IsNullOrWhiteSpace(_options.FromAddress);

        public async Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Email is not configured (see the \"Email\" section of appsettings).");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            var security = _options.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            using var client = new SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, security, cancellationToken);

            if (!string.IsNullOrEmpty(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
