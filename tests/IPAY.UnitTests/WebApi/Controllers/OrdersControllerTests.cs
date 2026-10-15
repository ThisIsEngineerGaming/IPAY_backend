using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class OrdersControllerTests
    {
        private Mock<IOrderService> _serviceMock = null!;
        private OrdersController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IOrderService>();
            _sut = new OrdersController(_serviceMock.Object).WithUser(8, "Customer");
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((OrderDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Create_OwnsTheOrderByTheTokenUser_AndReturnsCreatedAtAction()
        {
            var body = new CreateOrderDto { Items = { new CreateOrderItemDto { ProductId = 1, Quantity = 1 } } };
            _serviceMock.Setup(s => s.CreateAsync(body, 8)).ReturnsAsync(new OrderDto { Id = 21, UserId = 8 });

            var result = await _sut.Create(body);

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.ActionName, Is.EqualTo(nameof(OrdersController.GetById)));
            Assert.That(created.RouteValues!["id"], Is.EqualTo(21));
            _serviceMock.Verify(s => s.CreateAsync(body, 8), Times.Once);
        }

        [Test]
        public async Task Cancel_WhenOwnOrderCancellable_ReturnsOkWithOrder()
        {
            var order = new OrderDto { Id = 5, Status = "Cancelled" };
            _serviceMock.Setup(s => s.CancelAsync(5, 8)).ReturnsAsync(new CancelOrderOutcome(CancelOrderResult.Cancelled, order));

            var result = await _sut.Cancel(5);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(order));
        }

        [Test]
        public async Task Cancel_WhenAlreadyShipped_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.CancelAsync(5, 8)).ReturnsAsync(new CancelOrderOutcome(CancelOrderResult.NotCancellable));

            var result = await _sut.Cancel(5);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Cancel_WhenOrderBelongsToSomeoneElse_LooksLikeNotFound()
        {
            _serviceMock.Setup(s => s.CancelAsync(5, 8)).ReturnsAsync(new CancelOrderOutcome(CancelOrderResult.NotFound));

            var result = await _sut.Cancel(5);

            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateStatus_ReturnsNoContent()
        {
            var body = new UpdateOrderStatusDto { Status = IPAY.Domain.Enums.OrderStatus.Shipped };

            var result = await _sut.UpdateStatus(5, body);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateStatusAsync(5, body), Times.Once);
        }
    }
}
