using IPAY.Infrastructure.Payments;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Payments
{
    [TestFixture]
    public class MockPaymentProviderTests
    {
        private readonly MockPaymentProvider _sut = new();

        [Test]
        public void Name_IsMockPay()
        {
            Assert.That(_sut.Name, Is.EqualTo("MockPay"));
        }

        [Test]
        public async Task CreatePaymentAsync_ReturnsSuccessWithATwentyCharUppercaseTransactionId()
        {
            var result = await _sut.CreatePaymentAsync(42, 99.99m);

            Assert.That(result.Success, Is.True);
            Assert.That(result.TransactionId, Has.Length.EqualTo(20));
            Assert.That(result.TransactionId, Does.StartWith("MOCK-42-"));
            Assert.That(result.TransactionId, Is.EqualTo(result.TransactionId.ToUpperInvariant()));
        }

        [Test]
        public async Task CreatePaymentAsync_BuildsAConfirmUrlThatPointsBackAtTheSameOrderAndTransaction()
        {
            var result = await _sut.CreatePaymentAsync(42, 10m);

            Assert.That(result.PaymentUrl, Does.StartWith("/api/payments/mock/confirm"));
            Assert.That(result.PaymentUrl, Does.Contain($"transactionId={result.TransactionId}"));
            Assert.That(result.PaymentUrl, Does.Contain("orderId=42"));
        }

        [Test]
        public async Task CreatePaymentAsync_GivesDifferentTransactionIdsEachTime()
        {
            var first = await _sut.CreatePaymentAsync(1, 10m);
            var second = await _sut.CreatePaymentAsync(1, 10m);

            Assert.That(first.TransactionId, Is.Not.EqualTo(second.TransactionId));
        }

        [Test]
        public async Task ConfirmPaymentAsync_NormalTransaction_Succeeds()
        {
            var result = await _sut.ConfirmPaymentAsync("MOCK-1-ABC");

            Assert.That(result.Success, Is.True);
            Assert.That(result.TransactionId, Is.EqualTo("MOCK-1-ABC"));
            Assert.That(result.Message, Does.Contain("successful"));
        }

        [Test]
        public async Task ConfirmPaymentAsync_TransactionContainingFail_IsDeclined()
        {
            var result = await _sut.ConfirmPaymentAsync("MOCK-FAIL-1");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("failed"));
        }
    }
}
