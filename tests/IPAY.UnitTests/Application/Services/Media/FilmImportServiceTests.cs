using AutoMapper;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Mappings.Media;
using IPAY.Application.Services.Media;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Domain.Interfaces.ForRepos.Media;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Media
{
    [TestFixture]
    public class FilmImportServiceTests
    {
        private Mock<IOmdbService> _omdbMock = null!;
        private Mock<IFilmRepo> _filmsMock = null!;
        private Mock<IRepository<Genre>> _genresMock = null!;
        private FilmImportService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _omdbMock = new Mock<IOmdbService>();
            _filmsMock = new Mock<IFilmRepo>();
            _genresMock = new Mock<IRepository<Genre>>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MediaMapping>()).CreateMapper();
            _sut = new FilmImportService(_omdbMock.Object, _filmsMock.Object, _genresMock.Object, mapper);

            _filmsMock.Setup(r => r.GetByImdbIdAsync(It.IsAny<string>())).ReturnsAsync((Film?)null);
            _filmsMock.Setup(r => r.AddAsync(It.IsAny<Film>())).ReturnsAsync((Film f) => { f.Id = 7; return f; });
        }

        private static OmdbFilmDto Matrix(string type = "movie") => new()
        {
            Response = "True",
            Type = type,
            Title = "The Matrix",
            Plot = "A hacker learns the truth.",
            Year = "1999",
            Rated = "R",
            Director = "Lana Wachowski, Lilly Wachowski",
            Poster = "N/A",
            ImdbRating = "8.7",
            ImdbId = "tt0133093",
            Genre = "Action, Sci-Fi"
        };

        [Test]
        public async Task ImportAsync_LinksExistingGenres_AndNeverCreatesMissingOnes()
        {
            _omdbMock.Setup(s => s.GetByImdbIdAsync("tt0133093", It.IsAny<CancellationToken>())).ReturnsAsync(Matrix());
            _genresMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Genre> { new() { Id = 3, Name = "action" } });

            var result = await _sut.ImportAsync("tt0133093");

            Assert.That(result.Status, Is.EqualTo(FilmImportStatus.Imported));
            Assert.That(result.Film!.GenreIds, Is.EqualTo(new[] { 3 }));
            Assert.That(result.UnmatchedGenres, Is.EqualTo(new[] { "Sci-Fi" }));
            _genresMock.Verify(r => r.AddAsync(It.IsAny<Genre>()), Times.Never);
        }

        [Test]
        public async Task ImportAsync_MapsOmdbFields_AndTreatsNaAsEmpty()
        {
            _omdbMock.Setup(s => s.GetByImdbIdAsync("tt0133093", It.IsAny<CancellationToken>())).ReturnsAsync(Matrix());
            _genresMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Genre>());

            var result = await _sut.ImportAsync("tt0133093");

            Assert.That(result.Film!.Name, Is.EqualTo("The Matrix"));
            Assert.That(result.Film.Year, Is.EqualTo(1999));
            Assert.That(result.Film.Rating, Is.EqualTo(8.7));
            Assert.That(result.Film.AgeRating, Is.EqualTo("R"));
            Assert.That(result.Film.PosterUrl, Is.Empty);
            Assert.That(result.Film.ImdbId, Is.EqualTo("tt0133093"));
        }

        [Test]
        public async Task ImportAsync_WhenAlreadyImported_DoesNotCallOmdbOrSave()
        {
            _filmsMock.Setup(r => r.GetByImdbIdAsync("tt0133093")).ReturnsAsync(new Film { Id = 2, ImdbId = "tt0133093" });

            var result = await _sut.ImportAsync("tt0133093");

            Assert.That(result.Status, Is.EqualTo(FilmImportStatus.AlreadyImported));
            _omdbMock.Verify(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _filmsMock.Verify(r => r.AddAsync(It.IsAny<Film>()), Times.Never);
        }

        [Test]
        public async Task ImportAsync_WhenOmdbHasNoSuchFilm_ReturnsNotFound()
        {
            _omdbMock.Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbFilmDto { Response = "False", Error = "Incorrect IMDb ID." });

            var result = await _sut.ImportAsync("tt0000000");

            Assert.That(result.Status, Is.EqualTo(FilmImportStatus.NotFoundInOmdb));
        }

        [Test]
        public async Task ImportAsync_WhenTypeIsSeries_ReturnsNotAMovie()
        {
            _omdbMock.Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Matrix("series"));

            var result = await _sut.ImportAsync("tt0133093");

            Assert.That(result.Status, Is.EqualTo(FilmImportStatus.NotAMovie));
            _filmsMock.Verify(r => r.AddAsync(It.IsAny<Film>()), Times.Never);
        }
    }
}
