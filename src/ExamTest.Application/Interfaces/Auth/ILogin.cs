using ExamTest.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.Interfaces.Auth
{
    public interface ILogin
    {
        Task<AuthResponse> LoginAsync(LoginDto dto);
        Task<bool> Login(LoginDto dto);
    }
}
