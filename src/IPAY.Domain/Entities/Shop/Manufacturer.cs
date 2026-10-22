namespace IPAY.Domain.Entities.Shop
{
    //separate manufacturer class for product
    public class Manufacturer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
