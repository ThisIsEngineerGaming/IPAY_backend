using ExamTest.Domain.Entities.Shop;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase.Documents;

/// <summary>Firestore storage shape of <see cref="Manufacturer"/>.</summary>
[FirestoreData]
public class ManufacturerDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;

    public static ManufacturerDocument FromEntity(Manufacturer entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name
    };

    public Manufacturer ToEntity() => new()
    {
        Id = Id,
        Name = Name
    };
}
