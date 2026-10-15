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
    public class CartControllerTests
    {
        private Mock<ICartService> _serviceMock = null!;
        private CartController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<ICartService>();
            _sut = new CartController(_serviceMock.Object).WithUser(15, "Customer");
        }

        [Test]
        public async Task GetMyCart_UsesTheUserIdFromTheToken()
        {
            var cart = new CartDto { Id = 1 };
            _serviceMock.Setup(s => s.GetByUserIdAsync(15)).ReturnsAsync(cart);

            var result = await _sut.GetMyCart();

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(cart));
        }

        [Test]
        public async Task GetMyCart_WhenNoCartYet_ReturnsAnEmptyCartInsteadOfNotFound()
        {
            _serviceMock.Setup(s => s.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync((CartDto?)null);

            var result = await _sut.GetMyCart();

            var dto = (CartDto)((OkObjectResult)result.Result!).Value!;
            Assert.That(dto.Items, Is.Empty);
        }

        [Test]
        public async Task AddItem_PassesTokenUserAndBodyToService()
        {
            var body = new CreateCartItemDto { ProductId = 4, Quantity = 2 };
            _serviceMock.Setup(s => s.AddItemAsync(15, body)).ReturnsAsync(new CartDto { Id = 1 });

            var result = await _sut.AddItem(body);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _serviceMock.Verify(s => s.AddItemAsync(15, body), Times.Once);
        }

        [Test]
        public async Task UpdateItemQuantity_WhenItemNotInCart_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.UpdateItemQuantityAsync(15, 99, It.IsAny<CreateCartItemDto>())).ReturnsAsync((CartDto?)null);

            var result = await _sut.UpdateItemQuantity(99, new CreateCartItemDto { ProductId = 1, Quantity = 3 });

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task RemoveItem_WhenRemoved_ReturnsUpdatedCart()
        {
            _serviceMock.Setup(s => s.RemoveItemAsync(15, 2)).ReturnsAsync(new CartDto { Id = 1 });

            var result = await _sut.RemoveItem(2);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RemoveItem_WhenItemNotInCart_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.RemoveItemAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((CartDto?)null);

            var result = await _sut.RemoveItem(2);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task ClearCart_ReturnsNoContent()
        {
            var result = await _sut.ClearCart();

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.ClearCartAsync(15), Times.Once);
        }
    }
}
