namespace IPAY.Application.Interfaces.Auth
{
    public interface IGoogleLogin
    {
        /// <summary>
        /// Exchanges a Firebase ID token (from "Sign in with Google" on the client) for our own
        /// AuthResponse/JWT. Creates a Customer on first sign-in. Null when the token is invalid,
        /// the Google email is unverified, or the account is banned.
        /// </summary>
        Task<IPAY.Application.DTOs.Auth.AuthResponse?> LoginWithGoogleAsync(string idToken);
    }
}
