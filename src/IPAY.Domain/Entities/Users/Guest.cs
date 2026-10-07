using IPAY.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Entities.Users
{
    public  class Guest
    {
 
            public int? Id { get; set; }
            public string? Name { get; set; } = "Guest";
            public UserRole Role { get; set; } = UserRole.Guest;

            public bool IsBanned { get; set; } = false;

            // чтобы узнавать одного и того же гостя с разных запросов
            public string? SessionKey { get; set; }  // guid из cookie

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        

        public Guest()
        {
            Role = UserRole.Guest;
        }
    }
}
