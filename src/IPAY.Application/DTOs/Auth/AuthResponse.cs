using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }          // или int ExpiresIn (секунды)
        public string TokenType { get; set; } = "Bearer";

        // Информация о пользователе
        public LoginDto User { get; set; } = null!;

        // Two-factor step: when TwoFactorRequired is true there is NO token yet - the client must send
        // ChallengeId + the emailed code to /auth/login/verify-code to get the real AuthResponse.
        public bool TwoFactorRequired { get; set; }
        public string? ChallengeId { get; set; }
        public string? MaskedEmail { get; set; }
    }
}
