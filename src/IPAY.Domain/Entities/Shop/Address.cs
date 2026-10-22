namespace IPAY.Domain.Entities.Shop
{
    //class adress, which allows you to write it when you making an order(saves in db)
    public class Address
    {
        public int Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
