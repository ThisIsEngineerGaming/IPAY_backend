using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{
    public class UpdateSellerProductDto
    {
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        // Rating is not part of the request: it is the average of users' ratings (see ProductRatingService).
        public double DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int CategoryId { get; set; }


        public double DiscountPercent { get; set; }
    }
}
