using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{
    //creating cart item
    public class CreateCartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
