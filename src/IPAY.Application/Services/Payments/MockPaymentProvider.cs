// Infrastructure/Payments/MockPaymentProvider.cs
using IPAY.Application.Interfaces.Payments;

namespace IPAY.Infrastructure.Payments
{
    public class MockPaymentProvider : IPaymentProvider
    {
        public string Name => "MockPay";

        public Task<PaymentResult> CreatePaymentAsync(int orderId, decimal amount, string currency = "UAH")
        {
            // Генерируем фейковый id транзакции
            var txId = $"MOCK-{orderId}-{Guid.NewGuid():N}"[..20].ToUpper();

            // «Ссылка на оплату» — просто наш же endpoint подтверждения
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
            // Всегда успех (или можно сделать 90% success / 10% fail для демо)
            var success = !transactionId.Contains("FAIL"); // для теста можно передать FAIL в id

            return Task.FromResult(new PaymentResult
            {
                Success = success,
                TransactionId = transactionId,
                Message = success ? "Payment successful (mock)" : "Payment failed (mock)"
            });
        }
    }
}
