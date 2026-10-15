using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Auth;

namespace IPAY.Infrastructure.Persistence.Documents
{



        [FirestoreData]
        public class RefreshTokenDocument
        {
            [FirestoreProperty] public string Id { get; set; } = string.Empty;
            [FirestoreProperty] public int UserId { get; set; }
            [FirestoreProperty] public string Role { get; set; } = string.Empty;
            [FirestoreProperty] public string Email { get; set; } = string.Empty;
            [FirestoreProperty] public DateTime ExpiresAt { get; set; }
            [FirestoreProperty] public DateTime CreatedAt { get; set; }
            [FirestoreProperty] public bool IsRevoked { get; set; }
            [FirestoreProperty] public string? ReplacedByTokenId { get; set; }
            [FirestoreProperty] public string? CreatedByIp { get; set; }

            public static RefreshTokenDocument FromEntity(RefreshToken e) => new()
            {
                Id = e.Id,
                UserId = e.UserId,
                Role = e.Role,
                Email = e.Email,
                ExpiresAt = e.ExpiresAt,
                CreatedAt = e.CreatedAt,
                IsRevoked = e.IsRevoked,
                ReplacedByTokenId = e.ReplacedByTokenId,
                CreatedByIp = e.CreatedByIp
            };

            public RefreshToken ToEntity() => new()
            {
                Id = Id,
                UserId = UserId,
                Role = Role,
                Email = Email,
                ExpiresAt = ExpiresAt,
                CreatedAt = CreatedAt,
                IsRevoked = IsRevoked,
                ReplacedByTokenId = ReplacedByTokenId,
                CreatedByIp = CreatedByIp
            };
        }
    
}
