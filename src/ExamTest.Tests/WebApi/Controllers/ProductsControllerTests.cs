using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private Mock<IProductService> _serviceMock = null!;
        private ProductsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IProductService>();
            _sut = new ProductsController(_serviceMock.Object);
        }

        [Test]
        public async Task GetAll_ReturnsAllProductsFromService()
        {
            var products = new List<ProductDto> { new() { Id = 1, Name = "Phone" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

            var result = await _sut.GetAll();

            Assert.That(result, Is.SameAs(products));
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ProductDto { Id = 1, Name = "Phone" });

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ProductDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreatedAtActionWithProduct()
        {
            var request = new CreateAdminProductDto { Name = "New Product" };
            _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(new ProductDto { Id = 7, Name = "New Product" });

            var result = await _sut.Create(request);

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(((ProductDto)created!.Value!).Id, Is.EqualTo(7));
            Assert.That(created.ActionName, Is.EqualTo(nameof(ProductsController.GetById)));
        }

        [Test]
        public async Task Update_WhenProductExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ProductDto { Id = 1 });

            var result = await _sut.Update(1, new UpdateAdminProductDto { Name = "Updated" });

            Assert.That(result.Result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateAsync(1, It.IsAny<UpdateAdminProductDto>()), Times.Once);
        }

        [Test]
        public async Task Update_WhenProductMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ProductDto?)null);

            var result = await _sut.Update(99, new UpdateAdminProductDto());

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateAdminProductDto>()), Times.Never);
        }

        [Test]
        public async Task Delete_WhenProductExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ProductDto { Id = 1 });

            var result = await _sut.Delete(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task Delete_WhenProductMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ProductDto?)null);

            var result = await _sut.Delete(99);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetLimitProduct_UsesDefaultLimitOfTwelve()
        {
            _serviceMock.Setup(s => s.GetLimitAsync(12, null)).ReturnsAsync(new List<ProductDto>());

            var result = await _sut.GetLimitProduct();

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _serviceMock.Verify(s => s.GetLimitAsync(12, null), Times.Once);
        }

        [Test]
        public async Task GetLimitProduct_PassesCustomLimitAndCursorThrough()
        {
            _serviceMock.Setup(s => s.GetLimitAsync(5, "cursor")).ReturnsAsync(new List<ProductDto>());

            await _sut.GetLimitProduct(limit: 5, lastDocId: "cursor");

            _serviceMock.Verify(s => s.GetLimitAsync(5, "cursor"), Times.Once);
        }

        [Test]
        public async Task GetFiltered_PassesAllFiltersThrough()
        {
            _serviceMock
                .Setup(s => s.GetFilteredAsync(12, null, 2, 10, 100, "Acme", "phone"))
                .ReturnsAsync(new List<ProductDto?>());

            var result = await _sut.GetFiltered(categoryId: 2, minPrice: 10, maxPrice: 100, brand: "Acme", search: "phone");

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _serviceMock.Verify(s => s.GetFilteredAsync(12, null, 2, 10, 100, "Acme", "phone"), Times.Once);
        }

        [Test]
        public async Task GetSorted_UsesDefaultSortByPriceAscending()
        {
            _serviceMock.Setup(s => s.GetSortedAsync(12, null, "price", "asc")).ReturnsAsync(new List<ProductDto>());

            var result = await _sut.GetSorted();

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            _serviceMock.Verify(s => s.GetSortedAsync(12, null, "price", "asc"), Times.Once);
        }

        [Test]
        public async Task GetSorted_PassesCustomSortThrough()
        {
            _serviceMock.Setup(s => s.GetSortedAsync(20, "cursor", "rating", "desc")).ReturnsAsync(new List<ProductDto>());

            await _sut.GetSorted(sortBy: "rating", sortDir: "desc", limit: 20, lastDocId: "cursor");

            _serviceMock.Verify(s => s.GetSortedAsync(20, "cursor", "rating", "desc"), Times.Once);
        }
    }
}
