using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class GenresControllerTests
    {
        private Mock<IGenreService> _serviceMock = null!;
        private GenresController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IGenreService>();
            _sut = new GenresController(_serviceMock.Object);
        }

        [Test]
        public async Task GetAll_ReturnsAllGenresFromService()
        {
            var genres = new List<GenreDto> { new() { Id = 1, Name = "Action" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(genres);

            var result = await _sut.GetAll();

            Assert.That(result, Is.SameAs(genres));
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOk()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new GenreDto { Id = 1, Name = "Action" });

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GenreDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreatedAtActionWithGenre()
        {
            var request = new SaveGenreDto { Name = "Comedy" };
            _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(new GenreDto { Id = 2, Name = "Comedy" });

            var result = await _sut.Create(request);

            var created = result.Result as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(((GenreDto)created!.Value!).Id, Is.EqualTo(2));
        }

        [Test]
        public async Task Update_WhenGenreExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new GenreDto { Id = 1 });

            var result = await _sut.Update(1, new SaveGenreDto { Name = "Updated" });

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.UpdateAsync(1, It.IsAny<SaveGenreDto>()), Times.Once);
        }

        [Test]
        public async Task Update_WhenGenreMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GenreDto?)null);

            var result = await _sut.Update(99, new SaveGenreDto());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_WhenGenreExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new GenreDto { Id = 1 });

            var result = await _sut.Delete(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task Delete_WhenGenreMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GenreDto?)null);

            var result = await _sut.Delete(99);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AddSeries_WhenGenreExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new GenreDto { Id = 1 });

            var result = await _sut.AddSeries(1, 10);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.AddSeriesToGenreAsync(1, 10), Times.Once);
        }

        [Test]
        public async Task AddSeries_WhenGenreMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GenreDto?)null);

            var result = await _sut.AddSeries(99, 10);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task RemoveSeries_WhenGenreExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new GenreDto { Id = 1 });

            var result = await _sut.RemoveSeries(1, 10);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.RemoveSeriesFromGenreAsync(1, 10), Times.Once);
        }

        [Test]
        public async Task AddFilm_WhenGenreExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new GenreDto { Id = 1 });

            var result = await _sut.AddFilm(1, 5);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.AddFilmToGenreAsync(1, 5), Times.Once);
        }

        [Test]
        public async Task AddFilm_WhenGenreMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GenreDto?)null);

            var result = await _sut.AddFilm(99, 5);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task RemoveFilm_WhenGenreExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new GenreDto { Id = 1 });

            var result = await _sut.RemoveFilm(1, 5);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _serviceMock.Verify(s => s.RemoveFilmFromGenreAsync(1, 5), Times.Once);
        }

        [Test]
        public async Task RemoveFilm_WhenGenreMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GenreDto?)null);

            var result = await _sut.RemoveFilm(99, 5);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
