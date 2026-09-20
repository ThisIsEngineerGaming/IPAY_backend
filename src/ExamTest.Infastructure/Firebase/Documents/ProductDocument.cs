using ExamTest.Domain.Entities.Shop;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase.Documents;

/// <summary>Firestore storage shape of <see cref="Product"/>. Field names match the existing documents.</summary>
[FirestoreData]
public class ProductDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public double Price { get; set; }
    [FirestoreProperty] public double Rating { get; set; }
    [FirestoreProperty] public double DiscountedPrice { get; set; }
    [FirestoreProperty] public string ImageUrl { get; set; } = string.Empty;
    [FirestoreProperty] public string Manufacturer { get; set; } = string.Empty;
    [FirestoreProperty] public int CategoryId { get; set; }

    public static ProductDocument FromEntity(Product entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Price = entity.Price,
        Rating = entity.Rating,
        DiscountedPrice = entity.DiscountedPrice,
        ImageUrl = entity.ImageUrl,
        Manufacturer = entity.Manufacturer,
        CategoryId = entity.CategoryId
    };

    public Product ToEntity() => new()
    {
        Id = Id,
        Name = Name,
        Price = Price,
        Rating = Rating,
        DiscountedPrice = DiscountedPrice,
        ImageUrl = ImageUrl,
        Manufacturer = Manufacturer,
        CategoryId = CategoryId
    };
}
