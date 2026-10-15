using IPAY.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Auth
{
    public class GuestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Guest";
        public UserRole Role { get; set; } = UserRole.Guest;
        public bool IsBanned { get; set; }
        public string SessionKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
