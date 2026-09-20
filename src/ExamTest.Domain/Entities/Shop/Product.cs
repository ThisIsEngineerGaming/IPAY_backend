namespace ExamTest.Domain.Entities.Shop
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Money fields use double (not decimal) so the Firestore document model can store them
        // without a custom converter - same as Rating below.
        public double Price { get; set; }
        public double Rating { get; set; }
        public double DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;

        // Diagram showed "category:String" - using CategoryId instead so it actually
        // links to the Category entity below rather than duplicating its name as free text.
        public int CategoryId { get; set; }

        /// <summary>Percentage discount off the original price, e.g. 25 for 25% off.</summary>
        public double GetDiscountPercent()
        {
            if (Price <= 0) return 0;
            return (Price - DiscountedPrice) / Price * 100;
        }
    }
}
