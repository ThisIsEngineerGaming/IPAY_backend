
using IPAY.Application.Interfaces.Payments;

namespace IPAY.Infrastructure.Payments
{
    public class MockPaymentProvider : IPaymentProvider
    {
        public string Name => "MockPay";

        public Task<PaymentResult> CreatePaymentAsync(int orderId, decimal amount, string currency = "UAH")
        {
            // Fake id transaction
            var txId = $"MOCK-{orderId}-{Guid.NewGuid():N}"[..20].ToUpper();

            // «Link to order» 
            var paymentUrl = $"/api/payments/mock/confirm?transactionId={txId}&orderId={orderId}";

            return Task.FromResult(new PaymentResult
            {
                Success = true,
                TransactionId = txId,
                PaymentUrl = paymentUrl,
                Message = "Payment session created (mock)"
            });
        }

        public Task<PaymentResult> ConfirmPaymentAsync(string transactionId)
        {
            // Always succed
            var success = !transactionId.Contains("FAIL"); 

            return Task.FromResult(new PaymentResult
            {
                Success = success,
                TransactionId = transactionId,
                Message = success ? "Payment successful (mock)" : "Payment failed (mock)"
            });
        }
    }
}
