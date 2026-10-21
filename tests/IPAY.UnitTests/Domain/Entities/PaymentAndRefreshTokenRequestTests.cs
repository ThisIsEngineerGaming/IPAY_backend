using IPAY.Domain.Entities.Auth;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Enums;
using NUnit.Framework;

namespace IPAY.UnitTests.Domain.Entities;

[TestFixture]
public class PaymentAndRefreshTokenRequestTests
{
    [Test]
    public void NewPayment_IsPendingMockPayInUahCreatedNowAndNotPaid()
    {
        var before = DateTime.UtcNow;

        var payment = new Payment();

        Assert.Multiple(() =>
        {
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Pending));
            Assert.That(payment.Currency, Is.EqualTo("UAH"));
            Assert.That(payment.Provider, Is.EqualTo("MockPay"));
            Assert.That(payment.TransactionId, Is.Empty);
            Assert.That(payment.PaidAt, Is.Null);
            Assert.That(payment.CreatedAt, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(DateTime.UtcNow));
        });
    }

    [Test]
    public void Payment_KeepsAllAssignedValues()
    {
        var paidAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var payment = new Payment
        {
            Id = 7,
            OrderId = 12,
            Amount = 199.90m,
            Currency = "USD",
            Status = PaymentStatus.Success,
            Provider = "Stripe",
            TransactionId = "txn_1",
            PaidAt = paidAt
        };

        Assert.Multiple(() =>
        {
            Assert.That(payment.Id, Is.EqualTo(7));
            Assert.That(payment.OrderId, Is.EqualTo(12));
            Assert.That(payment.Amount, Is.EqualTo(199.90m));
            Assert.That(payment.Currency, Is.EqualTo("USD"));
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Success));
            Assert.That(payment.Provider, Is.EqualTo("Stripe"));
            Assert.That(payment.TransactionId, Is.EqualTo("txn_1"));
            Assert.That(payment.PaidAt, Is.EqualTo(paidAt));
        });
    }

    [Test]
    public void RefreshTokenRequest_DefaultsToEmptyAndKeepsTheToken()
    {
        Assert.That(new RefreshTokenRequest().RefreshToken, Is.Empty);
        Assert.That(new RefreshTokenRequest { RefreshToken = "abc" }.RefreshToken, Is.EqualTo("abc"));
    }
}
