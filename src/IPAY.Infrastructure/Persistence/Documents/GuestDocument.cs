
    using IPAY.Domain.Entities.Users;
    using IPAY.Domain.Enums;
    using Google.Cloud.Firestore;


namespace IPAY.Infrastructure.Persistence.Documents
{

    /// <summary>Firestore storage shape of <see cref="Guest"/>.</summary>
    [FirestoreData]
    public class GuestDocument
    {
        [FirestoreProperty] public int? Id { get; set; }
        [FirestoreProperty] public string? Name { get; set; } = "Guest";
        [FirestoreProperty] public UserRole Role { get; set; } = UserRole.Guest;
        [FirestoreProperty] public bool IsBanned { get; set; }
        [FirestoreProperty] public string? SessionKey { get; set; }
        [FirestoreProperty] public DateTime CreatedAt { get; set; }

        public static GuestDocument FromEntity(Guest entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Role = entity.Role,
            IsBanned = entity.IsBanned,
            SessionKey = entity.SessionKey,
            CreatedAt = entity.CreatedAt
        };

        public Guest ToEntity() => new()
        {
            Id = Id,
            Name = Name,
            Role = Role,
            IsBanned = IsBanned,
            SessionKey = SessionKey,
            CreatedAt = CreatedAt
        };
    }
}
