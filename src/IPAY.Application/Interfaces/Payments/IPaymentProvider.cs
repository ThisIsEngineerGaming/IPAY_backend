
namespace IPAY.Application.Interfaces.Payments
{
    public interface IPaymentProvider
    {
        string Name { get; } // "MockPay"
        Task<PaymentResult> CreatePaymentAsync(int orderId, decimal amount, string currency = "UAH");
        Task<PaymentResult> ConfirmPaymentAsync(string transactionId); // для webhook / callback
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; } // ссылка «на оплату»
        public string Message { get; set; } = string.Empty;
    }
}
