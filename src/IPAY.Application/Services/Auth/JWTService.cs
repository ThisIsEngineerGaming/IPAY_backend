using IPAY.Domain.Enums;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;


namespace IPAY.Application.Services.Auth
{
    public class JwtService : IJWT
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config) => _config = config;

        public string GenerateAccessToken(string userId, string email, UserRole role)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var minutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 20;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string HashToken(string rawToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(hash);
        }
    }
}
