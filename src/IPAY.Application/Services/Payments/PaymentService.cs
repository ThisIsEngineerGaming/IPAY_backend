using IPAY.Application.DTOs.Payments;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Payments;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Services.Payments
{
  
    public class PaymentService :IPaymentService
    {
        private readonly IPaymentProvider _provider;
        private readonly IOrderService _orderService;

        public PaymentService(IPaymentProvider paymentProvider, IOrderService orderService)
        {
            _provider = paymentProvider;
            _orderService = orderService;
        }
       

        public async Task<PaymentResponseDto> StartPaymentAsync(int orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException("Order not found");

            if (order.Status != "Pending")
                throw new InvalidOperationException("Order cannot be paid in current status");

            var result = await _provider.CreatePaymentAsync(orderId, order.TotalAmount);

            return new PaymentResponseDto
            {
                TransactionId = result.TransactionId,
                PaymentUrl = result.PaymentUrl,
                Status = "Pending",
                Message = result.Message,
                Provider = _provider.Name
            };
        }

        public async Task<PaymentResponseDto> ConfirmMockPaymentAsync(string transactionId, int orderId)
        {
            var result = await _provider.ConfirmPaymentAsync(transactionId);

            if (result.Success)
            {
                // Changes to paid
                await _orderService.UpdateStatusAsync(orderId, new UpdateOrderStatusDto
                {
                    Status = OrderStatus.Paid
                });
            }

            return new PaymentResponseDto
            {
                TransactionId = result.TransactionId,
                Status = result.Success ? "Success" : "Failed",
                Message = result.Message,
                Provider = _provider.Name
            };
        }
    }
}
