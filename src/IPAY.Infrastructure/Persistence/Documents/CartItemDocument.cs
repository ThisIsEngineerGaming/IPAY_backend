using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;

namespace IPAY.Domain.Entities.Shop
{
    [FirestoreData]
    public class CartItemDocument
    {
        [FirestoreProperty] public int Id { get; set; }
        [FirestoreProperty] public int ProductId { get; set; }
        [FirestoreProperty] public int Quantity { get; set; }

 

        public static CartItemDocument FromEntity(CartItem entity) => new()
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            Quantity = entity.Quantity
           
        };

        public CartItem ToEntity() => new()
        {
            Id = Id,
            ProductId = ProductId,
            Quantity = Quantity
        };
    }
}
