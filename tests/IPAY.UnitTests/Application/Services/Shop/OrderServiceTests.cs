using AutoMapper;
using FluentValidation;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Application.Mappings.Shop;
using IPAY.Application.Services.Shop;
using IPAY.Application.Validators.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Enums;
using IPAY.UnitTests.Support;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Shop
{
    [TestFixture]
    public class OrderServiceTests
    {
        private InMemoryOrderRepository _orders = null!;
        private InMemoryOrderItemRepository _orderItems = null!;
        private InMemoryProductRepo _products = null!;
        private OrderService _sut = null!;
        private int _phoneId;
        private int _caseId;

        [SetUp]
        public async Task SetUp()
        {
            _orders = new InMemoryOrderRepository();
            _orderItems = new InMemoryOrderItemRepository();
            _products = new InMemoryProductRepo();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<OrderMapping>()).CreateMapper();

            _sut = new OrderService(
                _orders, _orderItems, new InMemoryAddressRepository(), mapper,
                new CreateOrderDtoValidator(), new CreateOrderItemDtoValidator(), new UpdateOrderStatusDtoValidator(),
                _products, new InMemoryOrderStatusHistoryRepository());

            _phoneId = (await _products.AddProductAsync(new Product { Name = "Phone", Price = 100, DiscountedPrice = 75 })).Id;
            _caseId = (await _products.AddProductAsync(new Product { Name = "Case", Price = 10 })).Id;
        }

        private static CreateOrderDto Request(params (int productId, int qty)[] lines) => new()
        {
            Address = new AddressDto { Country = "UA", City = "Odesa", Street = "Deribasivska 1" },
            Items = lines.Select(l => new CreateOrderItemDto { ProductId = l.productId, Quantity = l.qty }).ToList()
        };

        [Test]
        public async Task CreateAsync_SnapshotsPriceAndNameFromTheProduct_NotFromTheRequest()
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 2), (_caseId, 1)), userId: 5);

            Assert.That(order.UserId, Is.EqualTo(5));
            Assert.That(order.Status, Is.EqualTo("Pending"));
            Assert.That(order.Items, Has.Count.EqualTo(2));
            Assert.That(order.Items.Single(i => i.ProductId == _phoneId).Price, Is.EqualTo(75m)); // discounted
            Assert.That(order.TotalAmount, Is.EqualTo(2 * 75m + 10m));
            Assert.That(order.TotalQuantity, Is.EqualTo(3));
        }

        [Test]
        public async Task CreateAsync_PriceChangeAfterwardsDoesNotChangeThePlacedOrder()
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 1)), 5);
            var phone = (await _products.GetProductByIdAsync(_phoneId))!;
            phone.DiscountedPrice = 0;
            phone.Price = 999;
            await _products.UpdateProductAsync(_phoneId, phone);

            var reloaded = await _sut.GetByIdAsync(order.Id);

            Assert.That(reloaded!.Items.Single().Price, Is.EqualTo(75m));
        }

        [Test]
        public void CreateAsync_WithNoItems_FailsValidation()
        {
            Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(Request(), 5));
        }

        [TestCase(0)]
        [TestCase(101)]
        public void CreateAsync_WithQuantityOutOfRange_FailsValidation(int quantity)
        {
            Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(Request((1, quantity)), 5));
        }

        [Test]
        public void CreateAsync_WithUnknownProduct_Throws()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(Request((9999, 1)), 5));
        }

        [Test]
        public async Task GetByUserIdAsync_ReturnsOnlyThatUsersOrders_WithTheirItems()
        {
            await _sut.CreateAsync(Request((_phoneId, 1)), 5);
            await _sut.CreateAsync(Request((_caseId, 2)), 6);

            var mine = await _sut.GetByUserIdAsync(5);

            Assert.That(mine, Has.Count.EqualTo(1));
            Assert.That(mine[0].Items.Single().ProductId, Is.EqualTo(_phoneId));
        }

        [Test]
        public async Task UpdateStatusAsync_ChangesTheStatus()
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 1)), 5);

            await _sut.UpdateStatusAsync(order.Id, new UpdateOrderStatusDto { Status = OrderStatus.Shipped });

            Assert.That((await _sut.GetByIdAsync(order.Id))!.Status, Is.EqualTo("Shipped"));
        }

        [Test]
        public void UpdateStatusAsync_ForUnknownOrder_Throws()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.UpdateStatusAsync(404, new UpdateOrderStatusDto { Status = OrderStatus.Paid }));
        }

        // ---------- cancelling ----------

        [Test]
        public async Task CancelAsync_OwnPendingOrder_IsCancelled()
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 1)), 5);

            var outcome = await _sut.CancelAsync(order.Id, 5);

            Assert.That(outcome.Result, Is.EqualTo(CancelOrderResult.Cancelled));
            Assert.That(outcome.Order!.Status, Is.EqualTo("Cancelled"));
            Assert.That((await _sut.GetByIdAsync(order.Id))!.Status, Is.EqualTo("Cancelled"));
        }

        [Test]
        public async Task CancelAsync_SomeoneElsesOrder_LooksLikeNotFound_AndChangesNothing()
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 1)), 5);

            var outcome = await _sut.CancelAsync(order.Id, userId: 6);

            Assert.That(outcome.Result, Is.EqualTo(CancelOrderResult.NotFound));
            Assert.That((await _sut.GetByIdAsync(order.Id))!.Status, Is.EqualTo("Pending"));
        }

        [TestCase(OrderStatus.Shipped)]
        [TestCase(OrderStatus.Delivered)]
        public async Task CancelAsync_AfterShipping_IsRefused(OrderStatus status)
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 1)), 5);
            await _sut.UpdateStatusAsync(order.Id, new UpdateOrderStatusDto { Status = status });

            var outcome = await _sut.CancelAsync(order.Id, 5);

            Assert.That(outcome.Result, Is.EqualTo(CancelOrderResult.NotCancellable));
            Assert.That((await _sut.GetByIdAsync(order.Id))!.Status, Is.EqualTo(status.ToString()));
        }

        [Test]
        public async Task CancelAsync_Twice_IsStillSuccess()
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 1)), 5);
            await _sut.CancelAsync(order.Id, 5);

            var again = await _sut.CancelAsync(order.Id, 5);

            Assert.That(again.Result, Is.EqualTo(CancelOrderResult.Cancelled));
        }

        [Test]
        public async Task CancelAsync_UnknownOrder_IsNotFound()
        {
            Assert.That((await _sut.CancelAsync(12345, 5)).Result, Is.EqualTo(CancelOrderResult.NotFound));
        }

        // ---------- items / delete ----------

        [Test]
        public async Task AddItemAsync_UsesTheProductsCurrentFinalPrice()
        {
            var order = await _sut.CreateAsync(Request((_caseId, 1)), 5);

            var item = await _sut.AddItemAsync(order.Id, new CreateOrderItemDto { ProductId = _phoneId, Quantity = 2 });

            Assert.That(item.Price, Is.EqualTo(75m));
            Assert.That(item.ProductName, Is.EqualTo("Phone"));
        }

        [Test]
        public void AddItemAsync_ToUnknownOrder_Throws()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.AddItemAsync(404, new CreateOrderItemDto { ProductId = _phoneId, Quantity = 1 }));
        }

        [Test]
        public async Task DeleteAsync_RemovesTheOrderAndItsItems()
        {
            var order = await _sut.CreateAsync(Request((_phoneId, 1), (_caseId, 1)), 5);

            await _sut.DeleteAsync(order.Id);

            Assert.That(await _sut.GetByIdAsync(order.Id), Is.Null);
            Assert.That(await _orderItems.GetByOrderIdAsync(order.Id), Is.Empty);
        }
    }
}
