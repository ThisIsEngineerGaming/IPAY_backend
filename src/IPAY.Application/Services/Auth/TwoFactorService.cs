using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Options;
using IPAY.Domain.Entities.Users;

namespace IPAY.Application.Services.Auth
{
    /// <summary>The code could not be emailed (SMTP down, wrong credentials...). AuthController returns 503.</summary>
    public class TwoFactorDeliveryException : Exception
    {
        public TwoFactorDeliveryException(Exception inner)
            : base("We could not send your sign-in code. Please try again in a moment.", inner)
        {
        }
    }

    public class TwoFactorService : ITwoFactorService
    {
        private static readonly Regex SixDigits = new(@"^\d{6}$", RegexOptions.Compiled);

        private readonly ITwoFactorStore _store;
        private readonly IEmailSender _email;
        private readonly IUser<Customer> _customers;
        private readonly IJWT _jwt;
        private readonly TwoFactorOptions _options;

        public TwoFactorService(
            ITwoFactorStore store,
            IEmailSender email,
            IUser<Customer> customers,
            IJWT jwt,
            TwoFactorOptions options)
        {
            _store = store;
            _email = email;
            _customers = customers;
            _jwt = jwt;
            _options = options;
        }

        // No SMTP server configured => nobody could receive a code, so don't lock everyone out.
        public bool IsEnabled => _options.Enabled && _email.IsConfigured;

        private TimeSpan Lifetime => TimeSpan.FromMinutes(Math.Max(1, _options.CodeLifetimeMinutes));

        public async Task<AuthResponse> BeginAsync(Customer user)
        {
            var challenge = new TwoFactorChallenge
            {
                Id = NewChallengeId(),
                UserId = user.Id!.Value,
                Email = user.Email!
            };

            var code = NewCode();
            ApplyCode(challenge, code);

            // Email first: if sending fails nothing is stored and the login simply errors out.
            await SendCodeAsync(challenge.Email, code);
            _store.Save(challenge, Lifetime);

            return new AuthResponse
            {
                TwoFactorRequired = true,
                ChallengeId = challenge.Id,
                MaskedEmail = Mask(challenge.Email)
            };
        }

        public async Task<TwoFactorResult<AuthResponse>> VerifyAsync(string challengeId, string code)
        {
            const string expired = "This code has expired. Please sign in again.";

            var challenge = string.IsNullOrWhiteSpace(challengeId) ? null : _store.Get(challengeId);
            if (challenge is null)
                return TwoFactorResult<AuthResponse>.Fail(expired);

            var now = DateTime.UtcNow;
            if (now > challenge.ExpiresAtUtc)
            {
                _store.Remove(challenge.Id);
                return TwoFactorResult<AuthResponse>.Fail(expired);
            }

            code = (code ?? string.Empty).Trim();
            if (!SixDigits.IsMatch(code))
                return TwoFactorResult<AuthResponse>.Fail("Enter the 6-digit code from the email.");

            if (!CodeMatches(challenge, code))
            {
                challenge.FailedAttempts++;

                if (challenge.FailedAttempts >= Math.Max(1, _options.MaxAttempts))
                {
                    _store.Remove(challenge.Id);
                    return TwoFactorResult<AuthResponse>.Fail("Too many wrong codes. Please sign in again.");
                }

                _store.Save(challenge, challenge.ExpiresAtUtc - now);
                var left = _options.MaxAttempts - challenge.FailedAttempts;
                return TwoFactorResult<AuthResponse>.Fail(
                    $"That code is not correct. {left} attempt{(left == 1 ? "" : "s")} left.");
            }

            // Right code - single use.
            _store.Remove(challenge.Id);

            var user = await _customers.GetByIdAsync(challenge.UserId);
            if (user is null || user.Id is null || string.IsNullOrWhiteSpace(user.Email) || user.IsBanned)
                return TwoFactorResult<AuthResponse>.Fail("Sign-in failed. Please try again.");

            var token = _jwt.GenerateToken(user.Id.Value.ToString(), user.Email, user.Role);

            return TwoFactorResult<AuthResponse>.Ok(new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                User = new LoginDto { Email = user.Email, Name = user.Name }
            });
        }

        public async Task<TwoFactorResult<string>> ResendAsync(string challengeId)
        {
            var challenge = string.IsNullOrWhiteSpace(challengeId) ? null : _store.Get(challengeId);
            if (challenge is null || DateTime.UtcNow > challenge.ExpiresAtUtc)
                return TwoFactorResult<string>.Fail("This sign-in has expired. Please sign in again.");

            var wait = challenge.LastSentAtUtc.AddSeconds(_options.ResendCooldownSeconds) - DateTime.UtcNow;
            if (wait > TimeSpan.Zero)
            {
                var seconds = (int)Math.Ceiling(wait.TotalSeconds);
                return TwoFactorResult<string>.Fail($"Please wait {seconds} seconds before requesting another code.");
            }

            // New code replaces the old one. FailedAttempts is deliberately NOT reset, otherwise
            // "resend" would hand out unlimited guesses.
            var code = NewCode();
            ApplyCode(challenge, code);

            await SendCodeAsync(challenge.Email, code);
            _store.Save(challenge, Lifetime);

            return TwoFactorResult<string>.Ok(Mask(challenge.Email));
        }

        // ---- helpers -------------------------------------------------------------------------

        private void ApplyCode(TwoFactorChallenge challenge, string code)
        {
            var now = DateTime.UtcNow;
            challenge.CodeHash = Hash(challenge.Id, code);
            challenge.LastSentAtUtc = now;
            challenge.ExpiresAtUtc = now + Lifetime;
        }

        private async Task SendCodeAsync(string toAddress, string code)
        {
            var minutes = (int)Lifetime.TotalMinutes;
            var body = EmailTemplates.SignInCode(code, minutes);

            try
            {
                await _email.SendAsync(toAddress, "Your IPAY sign-in code", body);
            }
            catch (Exception ex)
            {
                throw new TwoFactorDeliveryException(ex);
            }
        }

        private static string NewChallengeId() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        // Cryptographically random, uniformly distributed 000000-999999.
        private static string NewCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        private static string Hash(string challengeId, string code) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{challengeId}:{code}")));

        private static bool CodeMatches(TwoFactorChallenge challenge, string code)
        {
            var expected = Encoding.UTF8.GetBytes(challenge.CodeHash);
            var actual = Encoding.UTF8.GetBytes(Hash(challenge.Id, code));
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        private static string Mask(string email)
        {
            var at = email.IndexOf('@');
            return at <= 0 ? email : $"{email[0]}***{email[at..]}";
        }
    }
}
