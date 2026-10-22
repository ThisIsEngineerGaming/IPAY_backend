using IPAY.Domain.Enums;

namespace IPAY.Domain.Entities.Shop
{
    //class related with order history(history with changing status)
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? ChangedByUserId { get; set; } // who has changed
        public string? Comment { get; set; }      // reason
    }
}
