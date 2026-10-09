using IPAY.Domain.Enums;
using IPAY.Domain.Entities.Users;
using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Persistence.Documents;

/// <summary>Firestore storage shape of <see cref="Customer"/>.</summary>
[FirestoreData]
public class CustomerDocument
{
    [FirestoreProperty] public int? Id { get; set; }
    [FirestoreProperty] public string? Email { get; set; } = string.Empty;
    [FirestoreProperty] public string? Password { get; set; } = string.Empty;
    [FirestoreProperty] public string? Name { get; set; } = string.Empty;
    [FirestoreProperty] public bool IsBanned { get; set; }
    // Missing in documents created before email verification existed -> reads back as null.
    [FirestoreProperty] public bool? EmailVerified { get; set; }

    [FirestoreProperty] public int? PhoneNumber { get; set; } = null;

    [FirestoreProperty] public UserRole Role { get; set; }



    public static CustomerDocument FromEntity(Customer entity) => new()
    {
        Id = entity.Id,
        Email = entity.Email,
        Password = entity.Password,
        Name = entity.Name,
        IsBanned = entity.IsBanned,
        EmailVerified = entity.EmailVerified,
        Role = entity.Role
    };

    public Customer ToEntity() => new()
    {
        Id = Id,
        Email = Email,
        Password = Password,
        Name = Name,
        IsBanned = IsBanned,
        EmailVerified = EmailVerified,
        Role = Role
    };
}
