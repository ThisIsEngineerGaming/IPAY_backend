using IPAY.Domain.Enums;

namespace IPAY.Domain.Entities.Shop
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? ChangedByUserId { get; set; } // кто изменил (опционально)
        public string? Comment { get; set; }      // комментарий (опционально)
    }
}
