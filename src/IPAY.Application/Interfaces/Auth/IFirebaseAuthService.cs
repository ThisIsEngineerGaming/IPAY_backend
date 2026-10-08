namespace IPAY.Application.Interfaces.Auth
{
    /// <summary>
    /// What we trust about a user after Firebase has verified their ID token.
    /// AuthTime is when the person actually signed in to Firebase (the token's auth_time claim), null if absent.
    /// </summary>
    public record FirebaseIdentity(string Uid, string Email, string? Name, bool EmailVerified, DateTimeOffset? AuthTime = null);

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

        /// <summary>
        /// Makes sure a Firebase account exists for this email, so a password reset email can be sent from it.
        /// A missing account (older users never got one) is created with a random password nobody knows:
        /// the only way to get into it is the reset link emailed to the owner of the address.
        /// </summary>
        Task EnsureAccountAsync(string email);

        /// <summary>
        /// When the password of the Firebase account with this email was last changed (a reset revokes the
        /// account's sign-ins, which Firebase timestamps). Null when there is no account or it never changed.
        /// </summary>
        Task<DateTimeOffset?> GetPasswordChangedAtAsync(string email);
    }
}
