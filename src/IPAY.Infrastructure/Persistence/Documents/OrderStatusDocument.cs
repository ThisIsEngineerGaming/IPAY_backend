using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Enums;

namespace IPAY.Infrastructure.Persistence.Documents
{
    [FirestoreData]
    public class OrderStatusHistoryDocument
    {
        [FirestoreProperty] public int Id { get; set; }
        [FirestoreProperty] public int OrderID { get; set; }
        [FirestoreProperty] public string OldStatus { get; set; } = string.Empty;
        [FirestoreProperty] public string NewStatus { get; set; } = string.Empty;
        [FirestoreProperty] public DateTime ChangedAt { get; set; }
        [FirestoreProperty] public int? ChangedByUserId { get; set; }
        [FirestoreProperty] public string? Comment { get; set; }

        public static OrderStatusHistoryDocument FromEntity(OrderStatusHistory entity) => new()
        {
            Id = entity.Id,
            OrderID = entity.OrderId,
            OldStatus = entity.OldStatus.ToString(),
            NewStatus = entity.NewStatus.ToString(),
            ChangedAt = entity.ChangedAt,
            ChangedByUserId = entity.ChangedByUserId,
            Comment = entity.Comment
        };

        public OrderStatusHistory ToEntity() => new()
        {
            Id = Id,
            OrderId = OrderID,
            OldStatus = Enum.Parse<OrderStatus>(OldStatus),
            NewStatus = Enum.Parse<OrderStatus>(NewStatus),
            ChangedAt = ChangedAt,
            ChangedByUserId = ChangedByUserId,
            Comment = Comment
        };
    }
}
