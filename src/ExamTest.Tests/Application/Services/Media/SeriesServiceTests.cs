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
    public class SeriesServiceTests
    {
        private Mock<IRepository<Series>> _repositoryMock = null!;
        private IMapper _mapper = null!;
        private SeriesService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository<Series>>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MediaMapping>())
                .CreateMapper();
            _sut = new SeriesService(_repositoryMock.Object, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllSeriesMappedToDto()
        {
            var series = new List<Series>
            {
                new() { Id = 1, Name = "Show A", EpisodeIds = new List<int> { 1, 2 } },
                new() { Id = 2, Name = "Show B" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(series);

            var result = await _sut.GetAllAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Show A"));
            Assert.That(result[0].EpisodeIds, Is.EqualTo(new List<int> { 1, 2 }));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Series { Id = 1, Name = "Show A" });

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Show A"));
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Series?)null);

            var result = await _sut.GetByIdAsync(404);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_MapsAndReturnsCreatedDto()
        {
            var request = new SaveSeriesDto { Name = "New Show", Year = 2024 };
            _repositoryMock
                .Setup(r => r.AddAsync(It.Is<Series>(s => s.Name == "New Show")))
                .ReturnsAsync((Series s) => { s.Id = 9; return s; });

            var result = await _sut.CreateAsync(request);

            Assert.That(result.Id, Is.EqualTo(9));
            Assert.That(result.Name, Is.EqualTo("New Show"));
        }

        [Test]
        public async Task UpdateAsync_DelegatesToRepositoryWithMappedEntity()
        {
            var request = new SaveSeriesDto { Name = "Renamed Show" };

            await _sut.UpdateAsync(2, request);

            _repositoryMock.Verify(r => r.UpdateAsync(2, It.Is<Series>(s => s.Name == "Renamed Show")), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(8);

            _repositoryMock.Verify(r => r.DeleteAsync(8), Times.Once);
        }
    }
}
