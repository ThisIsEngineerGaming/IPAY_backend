using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using IPAY.Application.Interfaces.Auth;
using IPAY.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace IPAY.Infrastructure.Identity
{
    /// <summary>
    /// Firebase Authentication via the Admin SDK. Uses the same service account as Firestore
    /// (Firebase:ServiceAccountKeyPath), so no extra secret is needed.
    /// </summary>
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuth _auth;

        public bool RequireEmailVerification { get; }

        public FirebaseAuthService(IOptions<FirebaseOptions> options)
        {
            var settings = options.Value;
            RequireEmailVerification = settings.RequireEmailVerification;

            // FirebaseApp.Create throws if called twice, so reuse the default app when it exists.
            var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(settings.ServiceAccountKeyPath),
                ProjectId = settings.ProjectId
            });

            _auth = FirebaseAuth.GetAuth(app);
        }

        public async Task<FirebaseIdentity?> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                var token = await _auth.VerifyIdTokenAsync(idToken);

                string? Claim(string key) =>
                    token.Claims.TryGetValue(key, out var value) ? value?.ToString() : null;

                var email = Claim("email");
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                var verified = token.Claims.TryGetValue("email_verified", out var flag) &&
                               flag is bool b && b;

                return new FirebaseIdentity(token.Uid, email, Claim("name"), verified);
            }
            catch (FirebaseAuthException)
            {
                return null; // bad signature, expired, wrong project, revoked...
            }
            catch (ArgumentException)
            {
                return null; // malformed token string
            }
        }

        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            try
            {
                var user = await _auth.GetUserByEmailAsync(email);
                return user.EmailVerified;
            }
            catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                return false; // no Firebase account yet -> no verification email was ever confirmed
            }
        }
    }
}
