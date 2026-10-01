using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Mappings.Shop;
using IPAY.Application.Services.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Shop
{
    [TestFixture]
    public class ManufacturerServiceTests
    {
        private Mock<IRepository<Manufacturer>> _repositoryMock = null!;
        private ManufacturerService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository<Manufacturer>>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<ManufacturerMapping>()).CreateMapper();
            _sut = new ManufacturerService(_repositoryMock.Object, mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllManufacturersMappedToDto()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Manufacturer>
            {
                new() { Id = 1, Name = "Apple" },
                new() { Id = 2, Name = "Samsung" }
            });

            var result = await _sut.GetAllAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[1].Name, Is.EqualTo("Samsung"));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Manufacturer { Id = 1, Name = "Apple" });

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Apple"));
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Manufacturer?)null);

            Assert.That(await _sut.GetByIdAsync(404), Is.Null);
        }

        [Test]
        public async Task CreateAsync_AddsEntityAndReturnsMappedDto()
        {
            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Manufacturer>()))
                .ReturnsAsync((Manufacturer m) => { m.Id = 10; return m; });

            var result = await _sut.CreateAsync(new SaveManufacturerDto { Name = "Sony" });

            Assert.That(result.Id, Is.EqualTo(10));
            Assert.That(result.Name, Is.EqualTo("Sony"));
        }

        [Test]
        public async Task UpdateAsync_DelegatesToRepositoryWithMappedEntity()
        {
            await _sut.UpdateAsync(3, new SaveManufacturerDto { Name = "LG" });

            _repositoryMock.Verify(r => r.UpdateAsync(3, It.Is<Manufacturer>(m => m.Name == "LG")), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(3);

            _repositoryMock.Verify(r => r.DeleteAsync(3), Times.Once);
        }
    }
}
