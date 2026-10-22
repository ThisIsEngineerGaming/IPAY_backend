using IPAY.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Interfaces.Auth
{
    //login service
    public interface ILogin
    {
        Task<AuthResponse?> LoginAsync(LoginDto dto);
        Task<bool> Login(LoginDto dto);
    }
}
