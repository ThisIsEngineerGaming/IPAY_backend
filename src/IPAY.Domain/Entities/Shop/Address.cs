namespace IPAY.Domain.Entities.Shop
{
    public class Address
    {
        public int Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
