using System.Security.Claims;
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

        // ---- reading ----

        [Test]
        public async Task GetAll_ReturnsOkWithEveryOrder()
        {
            var orders = new List<OrderDto> { new() { Id = 1 }, new() { Id = 2 } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(orders);

            var result = await _sut.GetAll();

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(orders));
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOkWithOrder()
        {
            var order = new OrderDto { Id = 3 };
            _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(order);

            var result = await _sut.GetById(3);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(order));
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((OrderDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetByUserId_ReturnsThatUsersOrders()
        {
            var orders = new List<OrderDto> { new() { Id = 5, UserId = 77 } };
            _serviceMock.Setup(s => s.GetByUserIdAsync(77)).ReturnsAsync(orders);

            var result = await _sut.GetByUserId(77);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(orders));
        }

        [Test]
        public async Task GetStatusHistory_ReturnsTheHistoryOfThatOrder()
        {
            var history = new List<OrderStatusHistoryDto> { new() { Id = 1, OrderId = 5 } };
            _serviceMock.Setup(s => s.GetStatusHistoryAsync(5)).ReturnsAsync(history);

            var result = await _sut.GetStatusHistory(5);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(history));
        }

        // ---- my orders ----

        [Test]
        public async Task GetMyOrders_UsesTheIdFromTheToken()
        {
            var orders = new List<OrderDto> { new() { Id = 9, UserId = 8 } };
            _serviceMock.Setup(s => s.GetByUserIdAsync(8)).ReturnsAsync(orders);

            var result = await _sut.GetMyOrders();

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(orders));
        }

        [Test]
        public async Task GetMyOrders_FallsBackToTheSubClaim()
        {
            var sut = new OrdersController(_serviceMock.Object).WithClaims(new Claim("sub", "12"));
            _serviceMock.Setup(s => s.GetByUserIdAsync(12)).ReturnsAsync(new List<OrderDto>());

            var result = await sut.GetMyOrders();

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _serviceMock.Verify(s => s.GetByUserIdAsync(12), Times.Once);
        }

        [Test]
        public async Task GetMyOrders_WithoutAnIdClaim_ReturnsUnauthorized()
        {
            var sut = new OrdersController(_serviceMock.Object).WithUser(null, "Customer");

            var result = await sut.GetMyOrders();

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
            _serviceMock.Verify(s => s.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetMyOrders_WithANonNumericId_ReturnsUnauthorized()
        {
            var sut = new OrdersController(_serviceMock.Object)
                .WithClaims(new Claim(ClaimTypes.NameIdentifier, "not-a-number"));

            var result = await sut.GetMyOrders();

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        // ---- create ----

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
        public async Task Create_WithoutAnIdClaim_ReturnsUnauthorizedAndCreatesNothing()
        {
            var sut = new OrdersController(_serviceMock.Object).WithUser(null, "Customer");

            var result = await sut.Create(new CreateOrderDto());

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<CreateOrderDto>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task Create_FallsBackToTheSubClaim()
        {
            var sut = new OrdersController(_serviceMock.Object).WithClaims(new Claim("sub", "15"));
            var body = new CreateOrderDto();
            _serviceMock.Setup(s => s.CreateAsync(body, 15)).ReturnsAsync(new OrderDto { Id = 1, UserId = 15 });

            var result = await sut.Create(body);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        }

        // ---- cancel ----

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

        // ---- status / delete ----

        [Test]
        public async Task UpdateStatus_ReturnsNoContent()
        {
            var body = new UpdateOrderStatusDto { Status = IPAY.Domain.Enums.OrderStatus.Shipped };

            var result = await _sut.UpdateStatus(5, body);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateStatusAsync(5, body), Times.Once);
        }

        [Test]
        public async Task Delete_ReturnsNoContentAndDeletesTheOrder()
        {
            var result = await _sut.Delete(5);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteAsync(5), Times.Once);
        }

        // ---- items ----

        [Test]
        public async Task AddItem_ReturnsOkWithTheNewItem()
        {
            var body = new CreateOrderItemDto { ProductId = 2, Quantity = 3 };
            var item = new OrderItemDto { Id = 1, ProductId = 2, Quantity = 3 };
            _serviceMock.Setup(s => s.AddItemAsync(5, body)).ReturnsAsync(item);

            var result = await _sut.AddItem(5, body);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(item));
        }

        [Test]
        public async Task UpdateItem_ReturnsNoContentAndForwardsTheArguments()
        {
            var body = new CreateOrderItemDto { ProductId = 2, Quantity = 4 };

            var result = await _sut.UpdateItem(5, 6, body);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateItemAsync(5, 6, body), Times.Once);
        }

        [Test]
        public async Task DeleteItem_ReturnsNoContentAndForwardsTheArguments()
        {
            var result = await _sut.DeleteItem(5, 6);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteItemAsync(5, 6), Times.Once);
        }
    }
}
