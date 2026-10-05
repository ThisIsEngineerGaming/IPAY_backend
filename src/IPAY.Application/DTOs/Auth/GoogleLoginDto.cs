namespace IPAY.Application.DTOs.Auth
{
    public class GoogleLoginDto
    {
        /// <summary>Firebase ID token obtained on the client after signing in with Google.</summary>
        public string IdToken { get; set; } = string.Empty;
    }
}
