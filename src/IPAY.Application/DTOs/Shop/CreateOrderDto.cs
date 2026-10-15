using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{
    public class CreateOrderDto
    {
        public AddressDto Address { get; set; } = new();
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
