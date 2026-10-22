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
{   //JWT service
    public class JwtService : IJWT
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config) => _config = config;//represending as a set of key/value

        //generating the token
        public string GenerateAccessToken(string userId, string email, UserRole role)
        {
            //creating a claim object
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role.ToString())
            };
            //creating a structure of token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var minutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 20;//giving a limit in time
            //making a response token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);//returning a ready token
        }
        //token to update JWT
        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
        //hashing a token 
        public string HashToken(string rawToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(hash);
        }
    }
}
