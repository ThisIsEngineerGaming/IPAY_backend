using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Payments;
using IPAY.Application.Interfaces.Shop;
using IPAY.Application.Services.Payments;
using IPAY.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Payments
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private Mock<IPaymentProvider> _provider = null!;
        private Mock<IOrderService> _orders = null!;
        private PaymentService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _provider = new Mock<IPaymentProvider>();
            _provider.SetupGet(p => p.Name).Returns("TestPay");
            _orders = new Mock<IOrderService>();
            _sut = new PaymentService(_provider.Object, _orders.Object);
        }

        [Test]
        public void StartPaymentAsync_WhenOrderMissing_Throws()
        {
            _orders.Setup(o => o.GetByIdAsync(1)).ReturnsAsync((OrderDto?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.StartPaymentAsync(1));
        }

        [TestCase("Paid")]
        [TestCase("Shipped")]
        [TestCase("Delivered")]
        [TestCase("Cancelled")]
        public void StartPaymentAsync_WhenOrderIsNotPending_Throws(string status)
        {
            _orders.Setup(o => o.GetByIdAsync(1)).ReturnsAsync(new OrderDto { Id = 1, Status = status });

            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.StartPaymentAsync(1));
            _provider.Verify(p => p.CreatePaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task StartPaymentAsync_ForPendingOrder_ChargesTheOrderTotalAndReturnsProviderDetails()
        {
            _orders.Setup(o => o.GetByIdAsync(1)).ReturnsAsync(new OrderDto { Id = 1, Status = "Pending", TotalAmount = 250.5m });
            _provider.Setup(p => p.CreatePaymentAsync(1, 250.5m, It.IsAny<string>()))
                .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TX1", PaymentUrl = "/pay", Message = "ok" });

            var result = await _sut.StartPaymentAsync(1);

            Assert.That(result.TransactionId, Is.EqualTo("TX1"));
            Assert.That(result.PaymentUrl, Is.EqualTo("/pay"));
            Assert.That(result.Status, Is.EqualTo("Pending"));
            Assert.That(result.Message, Is.EqualTo("ok"));
            Assert.That(result.Provider, Is.EqualTo("TestPay"));
        }

        [Test]
        public async Task ConfirmMockPaymentAsync_OnSuccess_MarksTheOrderPaid()
        {
            _provider.Setup(p => p.ConfirmPaymentAsync("TX1"))
                .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TX1", Message = "done" });

            var result = await _sut.ConfirmMockPaymentAsync("TX1", 4);

            Assert.That(result.Status, Is.EqualTo("Success"));
            Assert.That(result.TransactionId, Is.EqualTo("TX1"));
            Assert.That(result.Provider, Is.EqualTo("TestPay"));
            _orders.Verify(o => o.UpdateStatusAsync(4, It.Is<UpdateOrderStatusDto>(d => d.Status == OrderStatus.Paid)), Times.Once);
        }

        [Test]
        public async Task ConfirmMockPaymentAsync_OnFailure_LeavesTheOrderAlone()
        {
            _provider.Setup(p => p.ConfirmPaymentAsync("TX1"))
                .ReturnsAsync(new PaymentResult { Success = false, TransactionId = "TX1", Message = "declined" });

            var result = await _sut.ConfirmMockPaymentAsync("TX1", 4);

            Assert.That(result.Status, Is.EqualTo("Failed"));
            Assert.That(result.Message, Is.EqualTo("declined"));
            _orders.Verify(o => o.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<UpdateOrderStatusDto>()), Times.Never);
        }
    }
}
