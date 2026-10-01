using IPAY.Domain.Enums;

namespace IPAY.Domain.Entities.Shop
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public Address Address { get; set; } = new();
        public List<OrderItem> Items { get; set; } = new();
    }
}
