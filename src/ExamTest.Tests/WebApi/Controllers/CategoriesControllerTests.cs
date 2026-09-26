using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class CategoriesControllerTests
    {
        private Mock<ICategoryService> _serviceMock = null!;
        private CategoriesController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<ICategoryService>();
            _sut = new CategoriesController(_serviceMock.Object);
        }

        [Test]
        public async Task GetAll_ReturnsAllCategoriesFromService()
        {
            var categories = new List<CategoryDto> { new() { Id = 1, Name = "Phones" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            var result = await _sut.GetAll();

            Assert.That(result, Is.SameAs(categories));
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new CategoryDto { Id = 1, Name = "Phones" });

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CategoryDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreatedAtActionWithCategory()
        {
            var request = new SaveCategoryDto { Name = "New" };
            _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(new CategoryDto { Id = 5, Name = "New" });

            var result = await _sut.Create(request);

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(((CategoryDto)created!.Value!).Id, Is.EqualTo(5));
            Assert.That(created.ActionName, Is.EqualTo(nameof(CategoriesController.GetById)));
        }

        [Test]
        public async Task Update_WhenCategoryExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new CategoryDto { Id = 1 });

            var result = await _sut.Update(1, new SaveCategoryDto { Name = "Updated" });

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateCategoryAsync(1, It.IsAny<SaveCategoryDto>()), Times.Once);
        }

        [Test]
        public async Task Update_WhenCategoryMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CategoryDto?)null);

            var result = await _sut.Update(99, new SaveCategoryDto());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _serviceMock.Verify(s => s.UpdateCategoryAsync(It.IsAny<int>(), It.IsAny<SaveCategoryDto>()), Times.Never);
        }

        [Test]
        public async Task Delete_WhenCategoryExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new CategoryDto { Id = 1 });

            var result = await _sut.Delete(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task Delete_WhenCategoryMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CategoryDto?)null);

            var result = await _sut.Delete(99);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AddProduct_WhenCategoryExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new CategoryDto { Id = 1 });

            var result = await _sut.AddProduct(1, 42);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.AddToCategoryAsync(1, 42), Times.Once);
        }

        [Test]
        public async Task AddProduct_WhenCategoryMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CategoryDto?)null);

            var result = await _sut.AddProduct(99, 1);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task RemoveProduct_WhenCategoryExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new CategoryDto { Id = 1 });

            var result = await _sut.RemoveProduct(1, 42);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.RemoveFromCategoryAsync(1, 42), Times.Once);
        }

        [Test]
        public async Task RemoveProduct_WhenCategoryMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((CategoryDto?)null);

            var result = await _sut.RemoveProduct(99, 1);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
