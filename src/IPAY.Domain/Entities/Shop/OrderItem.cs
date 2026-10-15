namespace IPAY.Domain.Entities.Shop
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
    }
}
