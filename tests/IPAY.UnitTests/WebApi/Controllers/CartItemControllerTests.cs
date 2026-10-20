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
    public class CartItemControllerTests
    {
        private Mock<ICartItemService> _service = null!;
        private CartItemController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ICartItemService>();
            _sut = new CartItemController(_service.Object).WithUser(9, "Customer");
        }

        [Test]
        public async Task GetById_ReturnsTheItemOfTheTokenUser()
        {
            var item = new CartItemDto { Id = 1 };
            _service.Setup(s => s.GetByIdAsync(9, 1)).ReturnsAsync(item);

            var result = await _sut.GetById(1);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(item));
        }

        [Test]
        public async Task GetById_WhenItemIsNotTheirs_ReturnsForbid()
        {
            _service.Setup(s => s.GetByIdAsync(9, 1)).ReturnsAsync((CartItemDto?)null);

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task Create_Returns201PointingAtTheNewItem()
        {
            var dto = new CreateCartItemDto { ProductId = 2, Quantity = 1 };
            _service.Setup(s => s.AddAsync(9, dto)).ReturnsAsync(new CartItemDto { Id = 33 });

            var result = await _sut.Create(dto);

            var created = (CreatedAtActionResult)result.Result!;
            Assert.That(created.RouteValues!["id"], Is.EqualTo(33));
        }

        [Test]
        public async Task Update_ReturnsTheUpdatedItem()
        {
            var dto = new CreateCartItemDto { ProductId = 2, Quantity = 4 };
            var updated = new CartItemDto { Id = 1, Quantity = 4 };
            _service.Setup(s => s.UpdateAsync(9, 1, dto)).ReturnsAsync(updated);

            var result = await _sut.Update(1, dto);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(updated));
        }

        [Test]
        public async Task Update_WhenItemIsNotTheirs_ReturnsForbid()
        {
            _service.Setup(s => s.UpdateAsync(9, 1, It.IsAny<CreateCartItemDto>())).ReturnsAsync((CartItemDto?)null);

            var result = await _sut.Update(1, new CreateCartItemDto());

            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task Delete_WhenDeleted_Returns204()
        {
            _service.Setup(s => s.DeleteAsync(9, 1)).ReturnsAsync(true);

            Assert.That(await _sut.Delete(1), Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Delete_WhenItemIsNotTheirs_ReturnsForbid()
        {
            _service.Setup(s => s.DeleteAsync(9, 1)).ReturnsAsync(false);

            Assert.That(await _sut.Delete(1), Is.InstanceOf<ForbidResult>());
        }
    }
}
