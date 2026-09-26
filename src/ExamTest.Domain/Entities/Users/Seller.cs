using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Domain.Entities.Auth
{
   public class Seller
    {
        public int? Id { get; set; }
        public string? Email { get; set; } = string.Empty;

        public string? Password { get; set; } = string.Empty;

        public string? Name { get; set; } = string.Empty;

        public bool IsBanned {  get; set; }=false;
    }
}
