using Google.Cloud.Firestore;

namespace ExamTest.Domain.Entities.Shop
{
    [FirestoreData]
    public class Product
    {
        [FirestoreProperty]
        public int Id { get; set; }
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        // Firestore's .NET client can't serialize System.Decimal without a custom converter,
        // so money fields use double here (same as Rating below).
        [FirestoreProperty]
        public double Price { get; set; }
        [FirestoreProperty]
        public double Rating { get; set; }
        [FirestoreProperty]
        public double DiscountedPrice { get; set; }
        [FirestoreProperty]
        public string ImageUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Manufacturer { get; set; } = string.Empty;

        // Diagram showed "category:String" - using CategoryId instead so it actually
        // links to the Category entity below rather than duplicating its name as free text.
        [FirestoreProperty]
        public int CategoryId { get; set; }

        /// <summary>Percentage discount off the original price, e.g. 25 for 25% off.</summary>
        public double GetDiscountPercent()
        {
            if (Price <= 0) return 0;
            return (Price - DiscountedPrice) / Price * 100;
        }
    }
}
