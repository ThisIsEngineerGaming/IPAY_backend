using IPAY.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Entities.Users
{
    public  class Guest
    {
        public int? Id { get; set; }
        public string? Name { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public Guest()
        {
            Role = UserRole.Guest;
        }
    }
}
