using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Mapping;
using IPAY.Application.Mappings.Shop;
using IPAY.Application.Services.Shop;
using IPAY.Application.Services.Shop.IPAY.Application.Services.Shop;
using IPAY.Application.Validators.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Enums;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace IPAY.UnitTests.Integration
{
    /// <summary>
    /// The money path end to end: a seller lists a product, a customer puts it in the cart and orders it,
    /// and the ownership rules hold along the way. Real controllers + services + validators + AutoMapper
    /// profiles; only the Firestore repositories are replaced by in-memory ones.
    /// </summary>
    [TestFixture]
    public class ShopFlowIntegrationTests
    {
        private InMemoryProductRepo _productRepo = null!;
        private InMemoryRepository<Cart> _cartRepo = null!;
        private ProductService _productService = null!;
        private CartService _cartService = null!;
        private OrderService _orderService = null!;
        private InMemoryOrderRepository _orderRepo = null!;

        private const int SellerA = 10;
        private const int SellerB = 11;
        private const int Customer1 = 20;
        private const int Customer2 = 21;

        [SetUp]
        public void SetUp()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductMapping>();
                cfg.AddProfile<CartMapping>();
                cfg.AddProfile<OrderMapping>();
            }).CreateMapper();

            _productRepo = new InMemoryProductRepo();
            _cartRepo = new InMemoryRepository<Cart>(c => c.Id, (c, id) => c.Id = id);
            _orderRepo = new InMemoryOrderRepository();

            _productService = new ProductService(
                _productRepo, mapper, new UpdateAdminProductDtoValidator(), new UpdateSellerProductDtoValidator());
            _cartService = new CartService(_cartRepo, _productRepo, mapper);
            _orderService = new OrderService(
                _orderRepo, new InMemoryOrderItemRepository(), new InMemoryAddressRepository(), mapper,
                new CreateOrderDtoValidator(), new CreateOrderItemDtoValidator(), new UpdateOrderStatusDtoValidator(),
                _productRepo, new InMemoryOrderStatusHistoryRepository());
        }

        private ProductsController Products(int userId, string role) =>
            new ProductsController(_productService).WithUser(userId, role);
        private CartController Cart(int userId) => new CartController(_cartService).WithUser(userId, "Customer");
        private OrdersController Orders(int userId, string role = "Customer") =>
            new OrdersController(_orderService).WithUser(userId, role);

        private static CreateSellerProductDto Mug(double price = 20, double discounted = 15) => new()
        {
            Name = "Mug", Price = price, DiscountedPrice = discounted,
            ImageUrl = "https://img.example/mug.png", Manufacturer = "Acme", CategoryId = 1,
            DiscountPercent = 25
        };

        private async Task<int> SellerListsAMug(int sellerId = SellerA)
        {
            var created = await Products(sellerId, "Seller").Create(Mug());
            return ((ProductDto)((CreatedAtActionResult)created.Result!).Value!).Id;
        }

        private static CreateOrderDto OrderFor(int productId, int quantity) => new()
        {
            Address = new AddressDto { Country = "UA", City = "Odesa", Street = "Deribasivska 1" },
            Items = { new CreateOrderItemDto { ProductId = productId, Quantity = quantity } }
        };

        // ---------- listing ----------

        [Test]
        public async Task SellerListing_IsStampedWithTheSellersIdFromTheToken_AndShowsUpInTheirOwnList()
        {
            var id = await SellerListsAMug(SellerA);

            var mine = await Products(SellerA, "Seller").GetMine();
            var others = await Products(SellerB, "Seller").GetMine();

            var myList = (IReadOnlyList<ProductDto>)((OkObjectResult)mine.Result!).Value!;
            Assert.That(myList.Single().Id, Is.EqualTo(id));
            Assert.That(myList.Single().SellerId, Is.EqualTo(SellerA.ToString()));
            Assert.That((IReadOnlyList<ProductDto>)((OkObjectResult)others.Result!).Value!, Is.Empty);
        }

        [Test]
        public async Task AnotherSeller_CannotEditOrDeleteMyProduct_AndItStaysUnchanged()
        {
            var id = await SellerListsAMug(SellerA);
            var intruder = Products(SellerB, "Seller");

            var edit = await intruder.Update(id, new UpdateSellerProductDto
            {
                Name = "Hijacked", Price = 1, DiscountedPrice = 0,
                ImageUrl = "https://img.example/x.png", Manufacturer = "Evil", CategoryId = 1
            });
            var delete = await intruder.Delete(id);

            Assert.That(edit.Result, Is.InstanceOf<ForbidResult>());
            Assert.That(delete, Is.InstanceOf<ForbidResult>());
            var stored = await _productService.GetByIdAsync(id);
            Assert.That(stored!.Name, Is.EqualTo("Mug"));
        }

        [Test]
        public async Task TheOwner_CanEditTheirProduct_WithoutResettingItsRating()
        {
            var id = await SellerListsAMug();
            var stored = (await _productRepo.GetProductByIdAsync(id))!;
            stored.Rating = 4.5;
            stored.RatingCount = 10;
            await _productRepo.UpdateProductAsync(id, stored);

            var edit = await Products(SellerA, "Seller").Update(id, new UpdateSellerProductDto
            {
                Name = "Big Mug", Price = 25, DiscountedPrice = 20,
                ImageUrl = "https://img.example/mug.png", Manufacturer = "Acme", CategoryId = 1
            });

            Assert.That(edit.Result, Is.InstanceOf<NoContentResult>());
            var after = await _productService.GetByIdAsync(id);
            Assert.That(after!.Name, Is.EqualTo("Big Mug"));
            Assert.That(after.Rating, Is.EqualTo(4.5));
            Assert.That(after.RatingCount, Is.EqualTo(10));
            Assert.That(after.SellerId, Is.EqualTo(SellerA.ToString()));
        }

        [Test]
        public async Task StaffCanDeleteAnyProduct_ASellerOnlyTheirOwn()
        {
            var id = await SellerListsAMug(SellerA);

            Assert.That(await Products(SellerB, "Seller").Delete(id), Is.InstanceOf<ForbidResult>());
            Assert.That(await Products(1, "Moderator").Delete(id), Is.InstanceOf<NoContentResult>());
            Assert.That(await _productService.GetByIdAsync(id), Is.Null);
        }

        // ---------- cart -> order ----------

        [Test]
        public async Task Customer_AddsToCart_ThenOrders_AtTheDiscountedPrice()
        {
            var productId = await SellerListsAMug();

            var cartResult = await Cart(Customer1).AddItem(new CreateCartItemDto { ProductId = productId, Quantity = 3 });
            var cart = (CartDto)((OkObjectResult)cartResult.Result!).Value!;
            Assert.That(cart.TotalPrice, Is.EqualTo(3 * 15)); // list price 20, customers pay 15

            var created = await Orders(Customer1).Create(OrderFor(productId, 3));
            var order = (OrderDto)((CreatedAtActionResult)created.Result!).Value!;

            Assert.That(order.UserId, Is.EqualTo(Customer1));
            Assert.That(order.Status, Is.EqualTo("Pending"));
            Assert.That(order.TotalAmount, Is.EqualTo(45m));
            Assert.That(order.Items.Single().ProductName, Is.EqualTo("Mug"));
        }

        [Test]
        public async Task OrderTotals_AreNotAffectedByALaterPriceChange()
        {
            var productId = await SellerListsAMug();
            var created = await Orders(Customer1).Create(OrderFor(productId, 2));
            var orderId = ((OrderDto)((CreatedAtActionResult)created.Result!).Value!).Id;

            await Products(1, "Admin").Update(productId, new UpdateAdminProductDto
            {
                Name = "Mug", Price = 500, DiscountedPrice = 400,
                ImageUrl = "https://img.example/mug.png", Manufacturer = "Acme", CategoryId = 1
            });

            var reloaded = await Orders(Customer1).GetById(orderId);
            var order = (OrderDto)((OkObjectResult)reloaded.Result!).Value!;
            Assert.That(order.TotalAmount, Is.EqualTo(30m));
        }

        [Test]
        public async Task EachCustomerHasTheirOwnCart()
        {
            var productId = await SellerListsAMug();
            await Cart(Customer1).AddItem(new CreateCartItemDto { ProductId = productId, Quantity = 1 });
            await Cart(Customer2).AddItem(new CreateCartItemDto { ProductId = productId, Quantity = 4 });

            var c1 = (CartDto)((OkObjectResult)(await Cart(Customer1).GetMyCart()).Result!).Value!;
            var c2 = (CartDto)((OkObjectResult)(await Cart(Customer2).GetMyCart()).Result!).Value!;

            Assert.That(c1.TotalItemsCount, Is.EqualTo(1));
            Assert.That(c2.TotalItemsCount, Is.EqualTo(4));
        }

        [Test]
        public void OrderingAProductThatDoesNotExist_Throws()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => Orders(Customer1).Create(OrderFor(9999, 1)));
        }

        // ---------- cancelling ----------

        [Test]
        public async Task Customer_CanCancelTheirOwnPendingOrder_ButNotSomeoneElses()
        {
            var productId = await SellerListsAMug();
            var created = await Orders(Customer1).Create(OrderFor(productId, 1));
            var orderId = ((OrderDto)((CreatedAtActionResult)created.Result!).Value!).Id;

            var byStranger = await Orders(Customer2).Cancel(orderId);
            var byOwner = await Orders(Customer1).Cancel(orderId);

            Assert.That(byStranger.Result, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(((OrderDto)((OkObjectResult)byOwner.Result!).Value!).Status, Is.EqualTo("Cancelled"));
        }

        [Test]
        public async Task ShippedOrder_CanNoLongerBeCancelledByTheCustomer()
        {
            var productId = await SellerListsAMug();
            var created = await Orders(Customer1).Create(OrderFor(productId, 1));
            var orderId = ((OrderDto)((CreatedAtActionResult)created.Result!).Value!).Id;
            await Orders(1, "Admin").UpdateStatus(orderId, new UpdateOrderStatusDto { Status = OrderStatus.Shipped });

            var result = await Orders(Customer1).Cancel(orderId);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That((await _orderRepo.GetByIdAsync(orderId))!.Status, Is.EqualTo(OrderStatus.Shipped));
        }
    }
}
