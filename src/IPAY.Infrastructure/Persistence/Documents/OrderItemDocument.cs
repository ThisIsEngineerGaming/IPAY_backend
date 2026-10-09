using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Infrastructure.Persistence.Documents
{
    [FirestoreData]
    public class OrderItemDocument
    {
        [FirestoreProperty] public int Id { get; set; }
        [FirestoreProperty] public int OrderId { get; set; }
        [FirestoreProperty] public int ProductId { get; set; }
        [FirestoreProperty] public int Quantity { get; set; }
        [FirestoreProperty] public double Price { get; set; }

        [FirestoreProperty] public string ProductName { get; set; } = string.Empty;

        public static OrderItemDocument FromEntity(OrderItem entity) => new()
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            ProductId = entity.ProductId,
            Quantity = entity.Quantity,
            Price = entity.Price,
            ProductName = entity.ProductName,
        };

        public OrderItem ToEntity() => new()
        {
            Id = Id,
            OrderId = OrderId,
            ProductId = ProductId,
            Quantity = Quantity,
            Price = Price,
            ProductName= ProductName
        };
    }
}
