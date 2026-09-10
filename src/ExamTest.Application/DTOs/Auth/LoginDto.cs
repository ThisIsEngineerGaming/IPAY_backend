using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.DTOs.Auth
{
    public  class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; }= string.Empty;
       
    }
}
