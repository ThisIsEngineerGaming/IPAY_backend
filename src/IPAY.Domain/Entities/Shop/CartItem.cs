namespace IPAY.Domain.Entities.Shop
{
    //the items in the cart(product but with additionall fields 
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
