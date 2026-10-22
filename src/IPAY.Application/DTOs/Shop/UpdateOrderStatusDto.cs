using IPAY.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{
    //updating order status(admin)
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }
}
