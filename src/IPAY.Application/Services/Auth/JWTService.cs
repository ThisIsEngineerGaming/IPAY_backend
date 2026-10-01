using IPAY.Domain.Enums;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IPAY.Application.Services.Auth
{
    public class JwtService:IJWT
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string userId, string email, UserRole userRole)
        {
            var claims = new List<Claim>
            {
              new Claim(ClaimTypes.NameIdentifier, userId),
              new Claim(ClaimTypes.Email, email),
              new Claim(ClaimTypes.Role, userRole.ToString())  // ← главное
            };


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),   // токен живёт 4 часа
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
