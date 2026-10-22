using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{

    //review dto

        public class ReviewDto
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int UserId { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    
}
