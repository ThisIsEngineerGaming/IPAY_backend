using IPAY.Application.DTOs.Auth;
using IPAY.Domain.Entities.Users;

namespace IPAY.Application.Interfaces.Auth
{
    /// <summary>A pending "type the code we emailed you" step between password and session.</summary>
    public class TwoFactorChallenge
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime LastSentAtUtc { get; set; }
        public int FailedAttempts { get; set; }
    }

    /// <summary>Short-lived storage for challenges (in memory - see Infrastructure/Caching).</summary>
    public interface ITwoFactorStore
    {
        void Save(TwoFactorChallenge challenge, TimeSpan lifetime);
        TwoFactorChallenge? Get(string challengeId);
        void Remove(string challengeId);
    }

    /// <summary>Outcome of a verify/resend call: either a value or a message safe to show the user.</summary>
    public record TwoFactorResult<T>(T? Value, string? Error)
    {
        public bool Succeeded => Error is null;
        public static TwoFactorResult<T> Ok(T value) => new(value, null);
        public static TwoFactorResult<T> Fail(string error) => new(default, error);
    }

    public interface ITwoFactorService
    {
        /// <summary>True when password logins must be followed by an emailed code.</summary>
        bool IsEnabled { get; }

        /// <summary>Creates a challenge, emails the code, and returns the "code required" response.</summary>
        Task<AuthResponse> BeginAsync(Customer user);

        /// <summary>Checks the typed code; on success returns the real login response (JWT).</summary>
        Task<TwoFactorResult<AuthResponse>> VerifyAsync(string challengeId, string code);

        /// <summary>Emails a fresh code for an existing challenge (rate limited).</summary>
        Task<TwoFactorResult<string>> ResendAsync(string challengeId);
    }
}
