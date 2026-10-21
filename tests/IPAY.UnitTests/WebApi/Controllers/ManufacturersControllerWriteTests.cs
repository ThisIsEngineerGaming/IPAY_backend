using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    /// <summary>The Update-success and Delete-missing branches that ManufacturersControllerTests does not reach.</summary>
    [TestFixture]
    public class ManufacturersControllerWriteTests
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
        public async Task Update_WhenFound_SavesTheChangesAndReturnsNoContent()
        {
            var body = new SaveManufacturerDto { Name = "Renamed" };
            _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(new ManufacturerDto { Id = 3, Name = "Old" });

            var result = await _sut.Update(3, body);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateAsync(3, body), Times.Once);
        }

        [Test]
        public async Task Delete_WhenMissing_ReturnsNotFoundAndDeletesNothing()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ManufacturerDto?)null);

            var result = await _sut.Delete(404);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _serviceMock.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task Create_PointsTheLocationAtGetByIdWithTheNewId()
        {
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<SaveManufacturerDto>()))
                .ReturnsAsync(new ManufacturerDto { Id = 9, Name = "Sony" });

            var result = await _sut.Create(new SaveManufacturerDto { Name = "Sony" });

            var created = (CreatedAtActionResult)result.Result!;
            Assert.That(created.ActionName, Is.EqualTo(nameof(ManufacturersController.GetById)));
            Assert.That(created.RouteValues!["id"], Is.EqualTo(9));
        }
    }
}
