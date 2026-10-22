using IPAY.Application.DTOs.Payments;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Interfaces.Payments
{
    //payment "status"
    public interface IPaymentService
    {
        Task<PaymentResponseDto> StartPaymentAsync(int orderId);
        Task<PaymentResponseDto> ConfirmMockPaymentAsync(string transactionId, int orderId);
    }
}
