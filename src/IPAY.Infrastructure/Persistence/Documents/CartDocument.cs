using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;

namespace IPAY.Domain.Entities.Shop
{
    [FirestoreData]
    public class CartDocument
    {
        [FirestoreProperty] public int Id { get; set; }
        [FirestoreProperty] public int UserId { get; set; }
        [FirestoreProperty] public List<CartItemDocument> Items { get; set; } = new();

        public static CartDocument FromEntity(Cart entity) => new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Items = entity.Items.Select(CartItemDocument.FromEntity).ToList()
        };

        public Cart ToEntity() => new()
        {
            Id = Id,
            UserId = UserId,
            Items = Items.Select(i => i.ToEntity()).ToList()
        };
    }
}
