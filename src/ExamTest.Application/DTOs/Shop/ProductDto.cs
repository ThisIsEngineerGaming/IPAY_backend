namespace ExamTest.Application.DTOs.Shop
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Rating { get; set; }
        public double DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        public double DiscountPercent { get; set; }
    }
}
