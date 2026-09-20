using ExamTest.Domain.Entities.Shop;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase.Documents;

/// <summary>Firestore storage shape of <see cref="Category"/>. Field names match the existing documents.</summary>
[FirestoreData]
public class CategoryDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public int Quantity { get; set; }
    [FirestoreProperty] public List<int> ProductIds { get; set; } = new();

    public static CategoryDocument FromEntity(Category entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Quantity = entity.Quantity,
        ProductIds = [.. entity.ProductIds]
    };

    public Category ToEntity() => new()
    {
        Id = Id,
        Name = Name,
        Quantity = Quantity,
        ProductIds = [.. ProductIds ?? []]
    };
}
