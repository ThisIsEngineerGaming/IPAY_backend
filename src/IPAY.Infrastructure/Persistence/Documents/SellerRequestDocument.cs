using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;

namespace IPAY.Infrastructure.Persistence.Documents;

/// <summary>Firestore storage shape of <see cref="SellerRequest"/>.</summary>
[FirestoreData]
public class SellerRequestDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public int UserId { get; set; }
    [FirestoreProperty] public string UserName { get; set; } = string.Empty;
    [FirestoreProperty] public string Email { get; set; } = string.Empty;
    [FirestoreProperty] public string Message { get; set; } = string.Empty;
    [FirestoreProperty] public SellerRequestStatus Status { get; set; }
    [FirestoreProperty] public DateTime CreatedAt { get; set; }
    [FirestoreProperty] public DateTime? ReviewedAt { get; set; }
    [FirestoreProperty] public string? ReviewedBy { get; set; }

    public static SellerRequestDocument FromEntity(SellerRequest entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        UserName = entity.UserName,
        Email = entity.Email,
        Message = entity.Message,
        Status = entity.Status,
        // Firestore only accepts UTC timestamps.
        CreatedAt = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc),
        ReviewedAt = entity.ReviewedAt.HasValue
            ? DateTime.SpecifyKind(entity.ReviewedAt.Value, DateTimeKind.Utc)
            : null,
        ReviewedBy = entity.ReviewedBy
    };

    public SellerRequest ToEntity() => new()
    {
        Id = Id,
        UserId = UserId,
        UserName = UserName,
        Email = Email,
        Message = Message,
        Status = Status,
        CreatedAt = CreatedAt,
        ReviewedAt = ReviewedAt,
        ReviewedBy = ReviewedBy
    };
}
