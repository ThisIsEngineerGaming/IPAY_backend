namespace IPAY.Application.DTOs.Shop
{
    //product dto (if we want to show in frontend)
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public double DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        public string SellerId { get; set; } = string.Empty;

        public double DiscountPercent { get; set; }
    }
}
