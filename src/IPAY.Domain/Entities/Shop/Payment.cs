// Domain/Entities/Shop/Payment.cs
using IPAY.Domain.Enums;

namespace IPAY.Domain.Entities.Shop
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "UAH";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string Provider { get; set; } = "MockPay"; // "LiqPay", "Mono", "Stripe"...
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
