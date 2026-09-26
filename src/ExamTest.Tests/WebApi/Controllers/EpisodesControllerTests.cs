using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class EpisodesControllerTests
    {
        private Mock<IEpisodeService> _serviceMock = null!;
        private EpisodesController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IEpisodeService>();
            _sut = new EpisodesController(_serviceMock.Object);
        }

        [Test]
        public async Task GetAll_WithoutSeriesId_ReturnsAllEpisodes()
        {
            var episodes = new List<EpisodeDto> { new() { Id = 1, Name = "Pilot" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(episodes);

            var result = await _sut.GetAll(seriesId: null);

            Assert.That(result, Is.SameAs(episodes));
            _serviceMock.Verify(s => s.GetBySeriesIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetAll_WithSeriesId_ReturnsEpisodesForThatSeries()
        {
            var episodes = new List<EpisodeDto> { new() { Id = 1, Name = "S1E1", SerialId = 10 } };
            _serviceMock.Setup(s => s.GetBySeriesIdAsync(10)).ReturnsAsync(episodes);

            var result = await _sut.GetAll(seriesId: 10);

            Assert.That(result, Is.SameAs(episodes));
            _serviceMock.Verify(s => s.GetAllAsync(), Times.Never);
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new EpisodeDto { Id = 1, Name = "Pilot" });

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((EpisodeDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreatedAtActionWithEpisode()
        {
            var request = new SaveEpisodeDto { Name = "New Episode" };
            _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(new EpisodeDto { Id = 3, Name = "New Episode" });

            var result = await _sut.Create(request);

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(((EpisodeDto)created!.Value!).Id, Is.EqualTo(3));
        }

        [Test]
        public async Task Update_WhenEpisodeExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new EpisodeDto { Id = 1 });

            var result = await _sut.Update(1, new SaveEpisodeDto { Name = "Updated" });

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateAsync(1, It.IsAny<SaveEpisodeDto>()), Times.Once);
        }

        [Test]
        public async Task Update_WhenEpisodeMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((EpisodeDto?)null);

            var result = await _sut.Update(99, new SaveEpisodeDto());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_WhenEpisodeExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new EpisodeDto { Id = 1 });

            var result = await _sut.Delete(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task Delete_WhenEpisodeMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((EpisodeDto?)null);

            var result = await _sut.Delete(99);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
