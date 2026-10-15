using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{

    public class CartDto
    {
        public int Id { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public double TotalPrice => Items.Sum(i => i.TotalPrice);
        public int TotalItemsCount => Items.Sum(i => i.Quantity);
    }
}
