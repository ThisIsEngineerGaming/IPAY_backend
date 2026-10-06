using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using IPAY.Application.Interfaces.Auth;
using IPAY.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<FirebaseAuthService> _logger;
        private readonly string _projectId;

        public bool RequireEmailVerification { get; }

        public FirebaseAuthService(IOptions<FirebaseOptions> options, ILogger<FirebaseAuthService> logger)
        {
            var settings = options.Value;
            _logger = logger;
            _projectId = settings.ProjectId;
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
                {
                    _logger.LogWarning("Google sign-in: the Firebase token has no \"email\" claim (uid {Uid}).", token.Uid);
                    return null;
                }

                // The claim can come back as a bool, a JsonElement or a string depending on the SDK's
                // JSON stack, so don't rely on `is bool` alone.
                var verified = token.Claims.TryGetValue("email_verified", out var flag) && IsTrue(flag);

                if (!verified)
                    _logger.LogWarning("Google sign-in: email_verified is not true for {Email} (raw claim: {Raw}).",
                        email, flag);

                return new FirebaseIdentity(token.Uid, email, Claim("name"), verified);
            }
            catch (FirebaseAuthException ex)
            {
                // bad signature, expired, wrong project, revoked...
                _logger.LogWarning(ex,
                    "Google sign-in: Firebase rejected the ID token (AuthErrorCode={Code}). " +
                    "Backend project is \"{ProjectId}\" - the frontend's Firebase config must use the same project.",
                    ex.AuthErrorCode, _projectId);
                return null;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Google sign-in: the ID token is malformed or empty.");
                return null; // malformed token string
            }
        }

        private static bool IsTrue(object? value) => value switch
        {
            bool b => b,
            string s => bool.TryParse(s, out var parsed) && parsed,
            System.Text.Json.JsonElement { ValueKind: System.Text.Json.JsonValueKind.True } => true,
            _ => false
        };

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
