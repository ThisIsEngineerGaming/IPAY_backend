namespace IPAY.Domain.Entities.Shop
{
    //class cart, which is saves in db
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
    }
}
