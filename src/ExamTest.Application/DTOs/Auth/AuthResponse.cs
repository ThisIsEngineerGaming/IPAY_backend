using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }          // или int ExpiresIn (секунды)
        public string TokenType { get; set; } = "Bearer";

        // Информация о пользователе
        public LoginDto User { get; set; } = null!;
    }
}
