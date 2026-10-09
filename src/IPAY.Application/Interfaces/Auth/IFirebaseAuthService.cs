namespace IPAY.Application.Interfaces.Auth
{
    /// <summary>What we trust about a user after Firebase has verified their ID token.</summary>
    public record FirebaseIdentity(string Uid, string Email, string? Name, bool EmailVerified);

    /// <summary>
    /// Thin wrapper around the Firebase Admin SDK (Firebase Authentication only - user data
    /// itself still lives in our own Firestore "customers" collection).
    /// </summary>
    public interface IFirebaseAuthService
    {
        /// <summary>When false, password login does not require a verified email (see Firebase:RequireEmailVerification).</summary>
        bool RequireEmailVerification { get; }

        /// <summary>Validates a Firebase ID token (signature, expiry, audience). Null when invalid.</summary>
        Task<FirebaseIdentity?> VerifyIdTokenAsync(string idToken);

        /// <summary>True when a Firebase account exists for this email AND its email is verified.</summary>
        Task<bool> IsEmailVerifiedAsync(string email);

        /// <summary>
        /// Sets a new password on the Firebase account that has this email, so Firebase stays in sync with
        /// the password we store. Returns false (and changes nothing) when there is no such account.
        /// </summary>
        Task<bool> SetPasswordAsync(string email, string newPassword);

        /// <summary>Deletes the Firebase account with this email. Does nothing when there is none.</summary>
        Task DeleteAccountAsync(string email);
    }
}
