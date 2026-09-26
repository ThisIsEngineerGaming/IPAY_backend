using AutoMapper;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Mappings.Shop;
using ExamTest.Application.Services.Shop;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Shop
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private Mock<IRepository<Category>> _repositoryMock = null!;
        private IMapper _mapper = null!;
        private CategoryService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository<Category>>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<CategoryMapping>())
                .CreateMapper();
            _sut = new CategoryService(_repositoryMock.Object, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllCategoriesMappedToDto()
        {
            var categories = new List<Category>
            {
                new() { Id = 1, Name = "Phones", Quantity = 2, ProductIds = new List<int> { 1, 2 } },
                new() { Id = 2, Name = "Laptops", Quantity = 0 }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

            var result = await _sut.GetAllAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Phones"));
            Assert.That(result[0].ProductIds, Is.EqualTo(new List<int> { 1, 2 }));
        }

        [Test]
        public async Task GetByIdAsync_WhenCategoryExists_ReturnsMappedDto()
        {
            var category = new Category { Id = 5, Name = "Tablets", Quantity = 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(category);

            var result = await _sut.GetByIdAsync(5);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(5));
            Assert.That(result.Name, Is.EqualTo("Tablets"));
        }

        [Test]
        public async Task GetByIdAsync_WhenCategoryDoesNotExist_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

            var result = await _sut.GetByIdAsync(404);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_MapsDtoToEntityAndReturnsCreatedDto()
        {
            var request = new SaveCategoryDto { Name = "Accessories", Quantity = 0 };
            _repositoryMock
                .Setup(r => r.AddAsync(It.Is<Category>(c => c.Name == "Accessories")))
                .ReturnsAsync((Category c) => { c.Id = 10; return c; });

            var result = await _sut.CreateAsync(request);

            Assert.That(result.Id, Is.EqualTo(10));
            Assert.That(result.Name, Is.EqualTo("Accessories"));
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        [Test]
        public async Task UpdateCategoryAsync_DelegatesToRepositoryWithMappedEntity()
        {
            var request = new SaveCategoryDto { Name = "Updated", Quantity = 3 };

            await _sut.UpdateCategoryAsync(7, request);

            _repositoryMock.Verify(r => r.UpdateAsync(7, It.Is<Category>(c => c.Name == "Updated" && c.Quantity == 3)), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(3);

            _repositoryMock.Verify(r => r.DeleteAsync(3), Times.Once);
        }

        [Test]
        public async Task AddToCategoryAsync_WhenCategoryExists_AddsProductAndUpdatesQuantity()
        {
            var category = new Category { Id = 1, Name = "Phones", ProductIds = new List<int> { 1 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            await _sut.AddToCategoryAsync(1, 2);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Category>(
                c => c.ProductIds.Contains(2) && c.Quantity == 2)), Times.Once);
        }

        [Test]
        public async Task AddToCategoryAsync_WhenProductAlreadyPresent_DoesNotDuplicateIt()
        {
            var category = new Category { Id = 1, Name = "Phones", ProductIds = new List<int> { 1, 2 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            await _sut.AddToCategoryAsync(1, 2);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Category>(
                c => c.ProductIds.Count == 2 && c.Quantity == 2)), Times.Once);
        }

        [Test]
        public async Task AddToCategoryAsync_WhenCategoryDoesNotExist_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

            await _sut.AddToCategoryAsync(99, 1);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Category>()), Times.Never);
        }

        [Test]
        public async Task RemoveFromCategoryAsync_RemovesProductAndUpdatesQuantity()
        {
            var category = new Category { Id = 1, Name = "Phones", ProductIds = new List<int> { 1, 2, 3 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            await _sut.RemoveFromCategoryAsync(1, 2);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Category>(
                c => !c.ProductIds.Contains(2) && c.Quantity == 2)), Times.Once);
        }

        [Test]
        public async Task RemoveFromCategoryAsync_WhenCategoryDoesNotExist_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

            await _sut.RemoveFromCategoryAsync(99, 1);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Category>()), Times.Never);
        }

        [Test]
        public async Task ClearCategoryAsync_ClearsProductsAndResetsQuantity()
        {
            var category = new Category { Id = 1, Name = "Phones", ProductIds = new List<int> { 1, 2, 3 }, Quantity = 3 };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            await _sut.ClearCategoryAsync(1);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Category>(
                c => c.ProductIds.Count == 0 && c.Quantity == 0)), Times.Once);
        }

        [Test]
        public async Task ClearCategoryAsync_WhenCategoryDoesNotExist_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

            await _sut.ClearCategoryAsync(42);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Category>()), Times.Never);
        }
    }
}
