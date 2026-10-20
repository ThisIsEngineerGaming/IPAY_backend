using IPAY.Application.DTOs.Payments;
using IPAY.Application.Interfaces.Payments;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class PaymentsControllerTests
    {
        private Mock<IPaymentService> _service = null!;
        private PaymentsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IPaymentService>();
            _sut = new PaymentsController(_service.Object);
        }

        [Test]
        public async Task Create_StartsAPaymentForTheOrder()
        {
            var response = new PaymentResponseDto { TransactionId = "TX1" };
            _service.Setup(s => s.StartPaymentAsync(5)).ReturnsAsync(response);

            var result = await _sut.Create(new CreatePaymentDto { OrderId = 5 });

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(response));
        }

        [Test]
        public async Task ConfirmMock_ConfirmsThePayment()
        {
            var response = new PaymentResponseDto { Status = "Success" };
            _service.Setup(s => s.ConfirmMockPaymentAsync("TX1", 5)).ReturnsAsync(response);

            var result = await _sut.ConfirmMock("TX1", 5);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(response));
        }
    }
}
