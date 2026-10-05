using IPAY.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }
}
