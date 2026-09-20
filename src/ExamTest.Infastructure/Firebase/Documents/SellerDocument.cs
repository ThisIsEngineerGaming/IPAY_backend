using ExamTest.Domain.Entities.Auth;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase.Documents;

/// <summary>Firestore storage shape of <see cref="Seller"/>.</summary>
[FirestoreData]
public class SellerDocument
{
    [FirestoreProperty] public int? Id { get; set; }
    [FirestoreProperty] public string? Email { get; set; } = string.Empty;
    [FirestoreProperty] public string? Password { get; set; } = string.Empty;
    [FirestoreProperty] public string? Name { get; set; } = string.Empty;
    [FirestoreProperty] public bool IsBanned { get; set; }

    public static SellerDocument FromEntity(Seller entity) => new()
    {
        Id = entity.Id,
        Email = entity.Email,
        Password = entity.Password,
        Name = entity.Name,
        IsBanned = entity.IsBanned
    };

    public Seller ToEntity() => new()
    {
        Id = Id,
        Email = Email,
        Password = Password,
        Name = Name,
        IsBanned = IsBanned
    };
}
