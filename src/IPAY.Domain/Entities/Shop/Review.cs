using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Entities.Shop
{


        public class Review
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int UserId { get; set; }
            public int Rating { get; set; }        
            public string Comment { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    
}
