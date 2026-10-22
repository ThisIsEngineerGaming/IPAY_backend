using IPAY.Domain.Enums;
using IPAY.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;



namespace IPAY.Application.Interfaces.Auth
{
    //JWT service interface (generation + hashing password)
    public interface IJWT
    {
        string GenerateAccessToken(string userId, string email, UserRole role);
        string GenerateRefreshToken(); 
        string HashToken(string rawToken); 
    }
}
