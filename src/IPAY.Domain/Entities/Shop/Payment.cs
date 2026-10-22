
using IPAY.Domain.Enums;

namespace IPAY.Domain.Entities.Shop
{
    //payment class(creates an "info payment")
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "UAH";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string Provider { get; set; } = "MockPay"; 
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
