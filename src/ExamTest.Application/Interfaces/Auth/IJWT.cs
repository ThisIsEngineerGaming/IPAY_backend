using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.Interfaces.Auth
{
    public  interface IJWT
    {
        public string GenerateToken(string userId, string email);
    }
}
