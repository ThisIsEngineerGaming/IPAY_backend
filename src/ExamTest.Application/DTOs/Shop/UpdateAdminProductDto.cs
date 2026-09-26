using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.DTOs.Shop
{
    public class UpdateAdminProductDto
    {
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
