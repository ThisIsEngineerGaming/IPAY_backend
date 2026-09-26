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
    public class FilmServiceTests
    {
        private Mock<IRepository<Film>> _repositoryMock = null!;
        private IMapper _mapper = null!;
        private FilmService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository<Film>>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MediaMapping>())
                .CreateMapper();
            _sut = new FilmService(_repositoryMock.Object, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllFilmsMappedToDto()
        {
            var films = new List<Film>
            {
                new() { Id = 1, Name = "Movie A", GenreIds = new List<int> { 1, 2 } },
                new() { Id = 2, Name = "Movie B" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(films);

            var result = await _sut.GetAllAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Movie A"));
            Assert.That(result[0].GenreIds, Is.EqualTo(new List<int> { 1, 2 }));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Film { Id = 1, Name = "Movie A" });

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Movie A"));
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Film?)null);

            var result = await _sut.GetByIdAsync(404);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_MapsAndReturnsCreatedDto()
        {
            var request = new SaveFilmDto { Name = "New Movie", Year = 2024, GenreIds = new List<int> { 3 } };
            _repositoryMock
                .Setup(r => r.AddAsync(It.Is<Film>(f => f.Name == "New Movie")))
                .ReturnsAsync((Film f) => { f.Id = 11; return f; });

            var result = await _sut.CreateAsync(request);

            Assert.That(result.Id, Is.EqualTo(11));
            Assert.That(result.Name, Is.EqualTo("New Movie"));
            Assert.That(result.Year, Is.EqualTo(2024));
        }

        [Test]
        public async Task UpdateAsync_DelegatesToRepositoryWithMappedEntity()
        {
            var request = new SaveFilmDto { Name = "Renamed Movie" };

            await _sut.UpdateAsync(2, request);

            _repositoryMock.Verify(r => r.UpdateAsync(2, It.Is<Film>(f => f.Name == "Renamed Movie")), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(6);

            _repositoryMock.Verify(r => r.DeleteAsync(6), Times.Once);
        }
    }
}
