using AutoMapper;
using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Mappings.Media;
using ExamTest.Application.Services.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Media
{
    [TestFixture]
    public class GenreServiceTests
    {
        private Mock<IRepository<Genre>> _repositoryMock = null!;
        private IMapper _mapper = null!;
        private GenreService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository<Genre>>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MediaMapping>())
                .CreateMapper();
            _sut = new GenreService(_repositoryMock.Object, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllGenresMappedToDto()
        {
            var genres = new List<Genre>
            {
                new() { Id = 1, Name = "Action", FilmIds = new List<int> { 1 } },
                new() { Id = 2, Name = "Drama" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(genres);

            var result = await _sut.GetAllAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Action"));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Genre { Id = 1, Name = "Action" });

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Action"));
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Genre?)null);

            var result = await _sut.GetByIdAsync(404);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_MapsAndReturnsCreatedDto()
        {
            var request = new SaveGenreDto { Name = "Comedy" };
            _repositoryMock
                .Setup(r => r.AddAsync(It.Is<Genre>(g => g.Name == "Comedy")))
                .ReturnsAsync((Genre g) => { g.Id = 3; return g; });

            var result = await _sut.CreateAsync(request);

            Assert.That(result.Id, Is.EqualTo(3));
            Assert.That(result.Name, Is.EqualTo("Comedy"));
        }

        [Test]
        public async Task UpdateAsync_DelegatesToRepositoryWithMappedEntity()
        {
            var request = new SaveGenreDto { Name = "Renamed" };

            await _sut.UpdateAsync(4, request);

            _repositoryMock.Verify(r => r.UpdateAsync(4, It.Is<Genre>(g => g.Name == "Renamed")), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(5);

            _repositoryMock.Verify(r => r.DeleteAsync(5), Times.Once);
        }

        [Test]
        public async Task AddSeriesToGenreAsync_AddsSeriesIdWhenNotPresent()
        {
            var genre = new Genre { Id = 1, SerialIds = new List<int> { 10 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);

            await _sut.AddSeriesToGenreAsync(1, 20);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Genre>(g => g.SerialIds.Contains(20))), Times.Once);
        }

        [Test]
        public async Task AddSeriesToGenreAsync_WhenAlreadyPresent_DoesNotDuplicate()
        {
            var genre = new Genre { Id = 1, SerialIds = new List<int> { 10, 20 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);

            await _sut.AddSeriesToGenreAsync(1, 20);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Genre>(g => g.SerialIds.Count == 2)), Times.Once);
        }

        [Test]
        public async Task AddSeriesToGenreAsync_WhenGenreMissing_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Genre?)null);

            await _sut.AddSeriesToGenreAsync(99, 1);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Genre>()), Times.Never);
        }

        [Test]
        public async Task AddFilmToGenreAsync_AddsFilmIdWhenNotPresent()
        {
            var genre = new Genre { Id = 1, FilmIds = new List<int>() };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);

            await _sut.AddFilmToGenreAsync(1, 7);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Genre>(g => g.FilmIds.Contains(7))), Times.Once);
        }

        [Test]
        public async Task AddFilmToGenreAsync_WhenGenreMissing_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Genre?)null);

            await _sut.AddFilmToGenreAsync(99, 1);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Genre>()), Times.Never);
        }

        [Test]
        public async Task RemoveSeriesFromGenreAsync_RemovesSeriesId()
        {
            var genre = new Genre { Id = 1, SerialIds = new List<int> { 10, 20 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);

            await _sut.RemoveSeriesFromGenreAsync(1, 10);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Genre>(g => !g.SerialIds.Contains(10))), Times.Once);
        }

        [Test]
        public async Task RemoveSeriesFromGenreAsync_WhenGenreMissing_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Genre?)null);

            await _sut.RemoveSeriesFromGenreAsync(99, 1);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Genre>()), Times.Never);
        }

        [Test]
        public async Task RemoveFilmFromGenreAsync_RemovesFilmId()
        {
            var genre = new Genre { Id = 1, FilmIds = new List<int> { 7, 8 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);

            await _sut.RemoveFilmFromGenreAsync(1, 7);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Genre>(g => !g.FilmIds.Contains(7))), Times.Once);
        }

        [Test]
        public async Task RemoveFilmFromGenreAsync_WhenGenreMissing_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Genre?)null);

            await _sut.RemoveFilmFromGenreAsync(99, 1);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Genre>()), Times.Never);
        }

        [Test]
        public async Task ClearGenreAsync_ClearsBothFilmAndSeriesIds()
        {
            var genre = new Genre { Id = 1, FilmIds = new List<int> { 1, 2 }, SerialIds = new List<int> { 3, 4 } };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);

            await _sut.ClearGenreAsync(1);

            _repositoryMock.Verify(r => r.UpdateAsync(1, It.Is<Genre>(
                g => g.FilmIds.Count == 0 && g.SerialIds.Count == 0)), Times.Once);
        }

        [Test]
        public async Task ClearGenreAsync_WhenGenreMissing_DoesNothing()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Genre?)null);

            await _sut.ClearGenreAsync(99);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Genre>()), Times.Never);
        }
    }
}
