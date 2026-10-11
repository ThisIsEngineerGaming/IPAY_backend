using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Payments
{
    public class PaymentResponseDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }
}
