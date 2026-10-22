using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Entities.Auth
{
        
        public class RefreshToken
        {
            /// <summary>Document id = SHA256 hash of the raw token (we never store the raw value).</summary>
            public string Id { get; set; } = string.Empty;

            public int UserId { get; set; }
            public string Role { get; set; } = string.Empty; // UserRole as string
            public string Email { get; set; } = string.Empty;

            public DateTime ExpiresAt { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public bool IsRevoked { get; set; }
            public string? ReplacedByTokenId { get; set; }
            public string? CreatedByIp { get; set; }

            public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;
        }
    
}
