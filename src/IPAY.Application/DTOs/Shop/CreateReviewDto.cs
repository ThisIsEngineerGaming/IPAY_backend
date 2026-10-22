using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{
      //dto related with writing a review

        public class CreateReviewDto
        {
            public int ProductId { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; } = string.Empty;
        }
    
}
