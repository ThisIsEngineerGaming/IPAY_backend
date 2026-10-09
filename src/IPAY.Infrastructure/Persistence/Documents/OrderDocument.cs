using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Infrastructure.Persistence.Documents
{
    [FirestoreData]
    public class OrderDocument
    {
        [FirestoreProperty] public int Id { get; set; }
        [FirestoreProperty] public int UserId { get; set; }
        [FirestoreProperty] public string Status { get; set; } = string.Empty;
        [FirestoreProperty] public DateTime CreatedAt { get; set; }
        [FirestoreProperty] public AddressDocument Address { get; set; } = new();
        [FirestoreProperty] public List<OrderItemDocument> Items { get; set; } = new();

        public static OrderDocument FromEntity(Order entity) => new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            Address = AddressDocument.FromEntity(entity.Address),
            Items = entity.Items.Select(OrderItemDocument.FromEntity).ToList()
        };

        public Order ToEntity() => new()
        {
            Id = Id,
            UserId = UserId,
            Status = Enum.Parse<OrderStatus>(Status),
            CreatedAt = CreatedAt,
            Address = Address.ToEntity(),
            Items = Items.Select(i => i.ToEntity()).ToList()
        };
    }
}
