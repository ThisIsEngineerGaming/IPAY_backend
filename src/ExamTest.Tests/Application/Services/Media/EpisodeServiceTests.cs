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
    public class EpisodeServiceTests
    {
        private Mock<IRepository<Episode>> _repositoryMock = null!;
        private IMapper _mapper = null!;
        private EpisodeService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository<Episode>>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MediaMapping>())
                .CreateMapper();
            _sut = new EpisodeService(_repositoryMock.Object, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllEpisodesMappedToDto()
        {
            var episodes = new List<Episode>
            {
                new() { Id = 1, Name = "Pilot", SerialId = 10 },
                new() { Id = 2, Name = "Episode 2", SerialId = 10 }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(episodes);

            var result = await _sut.GetAllAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Pilot"));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Episode { Id = 1, Name = "Pilot" });

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Pilot"));
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Episode?)null);

            var result = await _sut.GetByIdAsync(404);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBySeriesIdAsync_FiltersEpisodesBySerialId()
        {
            var episodes = new List<Episode>
            {
                new() { Id = 1, Name = "S1E1", SerialId = 10 },
                new() { Id = 2, Name = "S2E1", SerialId = 20 },
                new() { Id = 3, Name = "S1E2", SerialId = 10 }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(episodes);

            var result = await _sut.GetBySeriesIdAsync(10);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(e => e.Name), Is.EquivalentTo(new[] { "S1E1", "S1E2" }));
        }

        [Test]
        public async Task GetBySeriesIdAsync_WhenNoMatches_ReturnsEmptyList()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Episode>
            {
                new() { Id = 1, SerialId = 99 }
            });

            var result = await _sut.GetBySeriesIdAsync(1);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateAsync_MapsAndReturnsCreatedDto()
        {
            var request = new SaveEpisodeDto { Name = "New Episode", SerialId = 5 };
            _repositoryMock
                .Setup(r => r.AddAsync(It.Is<Episode>(e => e.Name == "New Episode")))
                .ReturnsAsync((Episode e) => { e.Id = 7; return e; });

            var result = await _sut.CreateAsync(request);

            Assert.That(result.Id, Is.EqualTo(7));
            Assert.That(result.Name, Is.EqualTo("New Episode"));
        }

        [Test]
        public async Task UpdateAsync_DelegatesToRepositoryWithMappedEntity()
        {
            var request = new SaveEpisodeDto { Name = "Renamed", SerialId = 5 };

            await _sut.UpdateAsync(3, request);

            _repositoryMock.Verify(r => r.UpdateAsync(3, It.Is<Episode>(e => e.Name == "Renamed")), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(9);

            _repositoryMock.Verify(r => r.DeleteAsync(9), Times.Once);
        }
    }
}
