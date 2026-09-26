using AutoMapper;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Mappings.Shop;
using ExamTest.Application.Services.Shop;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos.Shop;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Shop
{
    [TestFixture]
    public class ProductServiceTests
    {
        private Mock<IProductRepo> _repositoryMock = null!;
        private IMapper _mapper = null!;
        private ProductService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IProductRepo>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<ProductMapping>())
                .CreateMapper();
            _sut = new ProductService(_repositoryMock.Object, _mapper);
        }

        private static Product SampleProduct(int id = 1, double price = 100, double discounted = 75) => new()
        {
            Id = id,
            Name = "Phone",
            Price = price,
            DiscountedPrice = discounted,
            Rating = 4.5,
            Manufacturer = "Acme",
            CategoryId = 1,
            CreatedAt = new DateTime(2024, 1, 1)
        };

        [Test]
        public async Task GetAllAsync_ReturnsAllProductsMappedToDto()
        {
            var products = new List<Product> { SampleProduct(1), SampleProduct(2) };
            _repositoryMock.Setup(r => r.GetAllProductsAsync()).ReturnsAsync(products);

            var result = await _sut.GetAllAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].DiscountPercent, Is.EqualTo(25).Within(0.001));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            _repositoryMock.Setup(r => r.GetProductByIdAsync(1)).ReturnsAsync(SampleProduct(1));

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Phone"));
        }

        [Test]
        public async Task CreateAsync_SetsCreatedAtAndReturnsMappedDto()
        {
            var request = new CreateAdminProductDto
            {
                Name = "Laptop",
                Price = 1000,
                DiscountedPrice = 900,
                Manufacturer = "Acme",
                CategoryId = 2
            };

            Product? captured = null;
            _repositoryMock
                .Setup(r => r.AddProductAsync(It.IsAny<Product>()))
                .ReturnsAsync((Product p) => { captured = p; p.Id = 42; return p; });

            var result = await _sut.CreateAsync(request);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.CreatedAt, Is.Not.EqualTo(default(DateTime)));
            Assert.That(result.Id, Is.EqualTo(42));
            Assert.That(result.Name, Is.EqualTo("Laptop"));
        }

        [Test]
        public async Task UpdateAsync_WhenProductExistsAfterUpdate_ReturnsUpdatedDto()
        {
            var request = new UpdateAdminProductDto { Name = "Updated Phone", Price = 200, DiscountedPrice = 150 };
            _repositoryMock
                .Setup(r => r.GetProductByIdAsync(1))
                .ReturnsAsync(SampleProduct(1, price: 200, discounted: 150));

            var result = await _sut.UpdateAsync(1, request);

            Assert.That(result, Is.Not.Null);
            _repositoryMock.Verify(r => r.UpdateProductAsync(1, It.Is<Product>(p => p.Name == "Updated Phone")), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_WhenProductMissingAfterUpdate_ReturnsNull()
        {
            var request = new UpdateAdminProductDto { Name = "Ghost" };
            _repositoryMock.Setup(r => r.GetProductByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

            var result = await _sut.UpdateAsync(99, request);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(5);

            _repositoryMock.Verify(r => r.DeleteProductAsync(5), Times.Once);
        }

        [Test]
        public async Task GetLimitAsync_PassesLimitAndCursorThrough()
        {
            _repositoryMock
                .Setup(r => r.GetLimitedProduct(12, "abc"))
                .ReturnsAsync(new List<Product> { SampleProduct() });

            var result = await _sut.GetLimitAsync(12, "abc");

            Assert.That(result, Has.Count.EqualTo(1));
            _repositoryMock.Verify(r => r.GetLimitedProduct(12, "abc"), Times.Once);
        }

        [Test]
        public async Task GetFilteredAsync_PassesAllFiltersThrough()
        {
            _repositoryMock
                .Setup(r => r.GetFilteredAsync(12, null, 3, 10, 500, "Acme", "phone"))
                .ReturnsAsync(new List<Product> { SampleProduct() });

            var result = await _sut.GetFilteredAsync(12, null, 3, 10, 500, "Acme", "phone");

            Assert.That(result, Has.Count.EqualTo(1));
            _repositoryMock.Verify(r => r.GetFilteredAsync(12, null, 3, 10, 500, "Acme", "phone"), Times.Once);
        }

        [Test]
        public async Task GetSortedAsync_DefaultsToPriceAscending()
        {
            _repositoryMock
                .Setup(r => r.GetSortedAsync(12, null, "price", "asc"))
                .ReturnsAsync(new List<Product> { SampleProduct() });

            var result = await _sut.GetSortedAsync(12, null);

            Assert.That(result, Has.Count.EqualTo(1));
            _repositoryMock.Verify(r => r.GetSortedAsync(12, null, "price", "asc"), Times.Once);
        }

        [Test]
        public async Task GetSortedAsync_PassesCustomSortThrough()
        {
            _repositoryMock
                .Setup(r => r.GetSortedAsync(5, "cursor", "rating", "desc"))
                .ReturnsAsync(new List<Product>());

            await _sut.GetSortedAsync(5, "cursor", "rating", "desc");

            _repositoryMock.Verify(r => r.GetSortedAsync(5, "cursor", "rating", "desc"), Times.Once);
        }
    }
}
