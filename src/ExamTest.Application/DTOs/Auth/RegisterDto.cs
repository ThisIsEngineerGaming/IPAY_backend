using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ExamTest.Application.DTOs.Auth
{
    public  class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword {  get; set; } = string.Empty;
        public string Name = "guest";
    }
}
