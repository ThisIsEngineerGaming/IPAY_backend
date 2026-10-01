using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class ManufacturersControllerTests
    {
        private Mock<IManufacturerService> _serviceMock = null!;
        private ManufacturersController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IManufacturerService>();
            _sut = new ManufacturersController(_serviceMock.Object);
        }

        [Test]
        public async Task GetAll_ReturnsAllManufacturersFromService()
        {
            var items = new List<ManufacturerDto> { new() { Id = 1, Name = "Apple" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

            var result = await _sut.GetAll();

            Assert.That(result, Is.SameAs(items));
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ManufacturerDto { Id = 1, Name = "Apple" });

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ManufacturerDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreatedAtActionWithCreatedDto()
        {
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<SaveManufacturerDto>()))
                .ReturnsAsync(new ManufacturerDto { Id = 9, Name = "Sony" });

            var result = await _sut.Create(new SaveManufacturerDto { Name = "Sony" });

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task Update_WhenMissing_ReturnsNotFoundAndDoesNotUpdate()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ManufacturerDto?)null);

            var result = await _sut.Update(404, new SaveManufacturerDto { Name = "x" });

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<SaveManufacturerDto>()), Times.Never);
        }

        [Test]
        public async Task Delete_WhenFound_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ManufacturerDto { Id = 1 });

            var result = await _sut.Delete(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }
    }
}
