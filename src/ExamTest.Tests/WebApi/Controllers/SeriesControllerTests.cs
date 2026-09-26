using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class SeriesControllerTests
    {
        private Mock<ISeriesService> _serviceMock = null!;
        private SeriesController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<ISeriesService>();
            _sut = new SeriesController(_serviceMock.Object);
        }

        [Test]
        public async Task GetAll_ReturnsAllSeriesFromService()
        {
            var series = new List<SeriesDto> { new() { Id = 1, Name = "Show A" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(series);

            var result = await _sut.GetAll();

            Assert.That(result, Is.SameAs(series));
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new SeriesDto { Id = 1, Name = "Show A" });

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SeriesDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreatedAtActionWithSeries()
        {
            var request = new SaveSeriesDto { Name = "New Show" };
            _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(new SeriesDto { Id = 6, Name = "New Show" });

            var result = await _sut.Create(request);

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(((SeriesDto)created!.Value!).Id, Is.EqualTo(6));
        }

        [Test]
        public async Task Update_WhenSeriesExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new SeriesDto { Id = 1 });

            var result = await _sut.Update(1, new SaveSeriesDto { Name = "Updated" });

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateAsync(1, It.IsAny<SaveSeriesDto>()), Times.Once);
        }

        [Test]
        public async Task Update_WhenSeriesMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SeriesDto?)null);

            var result = await _sut.Update(99, new SaveSeriesDto());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_WhenSeriesExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new SeriesDto { Id = 1 });

            var result = await _sut.Delete(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task Delete_WhenSeriesMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SeriesDto?)null);

            var result = await _sut.Delete(99);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
