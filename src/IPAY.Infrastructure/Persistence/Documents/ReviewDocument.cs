


using IPAY.Domain.Entities.Shop;
using Google.Cloud.Firestore;


namespace IPAY.Infrastructure.Persistence.Documents
    {
        [FirestoreData]
        public class ReviewDocument
        {
            [FirestoreProperty] public int Id { get; set; }
            [FirestoreProperty] public int ProductId { get; set; }
            [FirestoreProperty] public int UserId { get; set; }
            [FirestoreProperty] public int Rating { get; set; }
            [FirestoreProperty] public string Comment { get; set; } = string.Empty;
            [FirestoreProperty] public DateTime CreatedAt { get; set; }

            public static ReviewDocument FromEntity(Review entity) => new()
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                UserId = entity.UserId,
                Rating = entity.Rating,
                Comment = entity.Comment,
                CreatedAt = entity.CreatedAt
            };

            public Review ToEntity() => new()
            {
                Id = Id,
                ProductId = ProductId,
                UserId = UserId,
                Rating = Rating,
                Comment = Comment,
                CreatedAt = CreatedAt
            };
        }
    }

