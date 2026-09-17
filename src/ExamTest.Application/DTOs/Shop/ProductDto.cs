using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.DTOs.Shop
{
    public  class ProductDto
    {
        
      
       
        public string Name { get; set; } = string.Empty;

        // Firestore's .NET client can't serialize System.Decimal without a custom converter,
        // so money fields use double here (same as Rating below).
      
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
