using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    // The OMDb lookups (api/films/omdb/...) are tested in OmdbControllerTests.
    [TestFixture]
    public class FilmsControllerTests
    {
        private const string ValidImdbId = "tt0133093";

        private Mock<IFilmService> _filmsMock = null!;
        private Mock<IFilmImportService> _importerMock = null!;
        private FilmsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _filmsMock = new Mock<IFilmService>();
            _importerMock = new Mock<IFilmImportService>();
            _sut = new FilmsController(_filmsMock.Object, _importerMock.Object);
        }

        // ---- catalog ----

        [Test]
        public async Task GetPage_PassesEveryQueryValueToTheServiceAndReturnsOk()
        {
            var page = new List<FilmDto> { new() { Id = 1, Name = "Matrix" } };
            _filmsMock.Setup(f => f.GetPageAsync(5, "9", 3, "mat", "rating", "desc")).ReturnsAsync(page);

            var result = await _sut.GetPage(5, "9", 3, "mat", "rating", "desc");

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(page));
        }

        [Test]
        public async Task GetPage_UsesTwentyAsTheDefaultPageSize()
        {
            _filmsMock.Setup(f => f.GetPageAsync(20, null, null, null, null, null)).ReturnsAsync(new List<FilmDto>());

            await _sut.GetPage();

            _filmsMock.Verify(f => f.GetPageAsync(20, null, null, null, null, null), Times.Once);
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsOkWithFilm()
        {
            var film = new FilmDto { Id = 4, Name = "Alien" };
            _filmsMock.Setup(f => f.GetByIdAsync(4)).ReturnsAsync(film);

            var result = await _sut.GetById(4);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(film));
        }

        [Test]
        public async Task GetById_WhenMissing_ReturnsNotFound()
        {
            _filmsMock.Setup(f => f.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((FilmDto?)null);

            var result = await _sut.GetById(404);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreatedAtGetByIdWithTheNewId()
        {
            var body = new SaveFilmDto { Name = "Heat" };
            _filmsMock.Setup(f => f.CreateAsync(body)).ReturnsAsync(new FilmDto { Id = 11, Name = "Heat" });

            var result = await _sut.Create(body);

            var created = (CreatedAtActionResult)result.Result!;
            Assert.That(created.ActionName, Is.EqualTo(nameof(FilmsController.GetById)));
            Assert.That(created.RouteValues!["id"], Is.EqualTo(11));
        }

        [Test]
        public async Task Update_WhenMissing_ReturnsNotFoundAndDoesNotSave()
        {
            _filmsMock.Setup(f => f.GetByIdAsync(5)).ReturnsAsync((FilmDto?)null);

            var result = await _sut.Update(5, new SaveFilmDto());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _filmsMock.Verify(f => f.UpdateAsync(It.IsAny<int>(), It.IsAny<SaveFilmDto>()), Times.Never);
        }

        [Test]
        public async Task Update_WhenFound_SavesAndReturnsNoContent()
        {
            var body = new SaveFilmDto { Name = "New name" };
            _filmsMock.Setup(f => f.GetByIdAsync(5)).ReturnsAsync(new FilmDto { Id = 5 });

            var result = await _sut.Update(5, body);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _filmsMock.Verify(f => f.UpdateAsync(5, body), Times.Once);
        }

        [Test]
        public async Task Delete_WhenMissing_ReturnsNotFoundAndDoesNotDelete()
        {
            _filmsMock.Setup(f => f.GetByIdAsync(5)).ReturnsAsync((FilmDto?)null);

            var result = await _sut.Delete(5);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _filmsMock.Verify(f => f.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task Delete_WhenFound_DeletesAndReturnsNoContent()
        {
            _filmsMock.Setup(f => f.GetByIdAsync(5)).ReturnsAsync(new FilmDto { Id = 5 });

            var result = await _sut.Delete(5);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _filmsMock.Verify(f => f.DeleteAsync(5), Times.Once);
        }

        // ---- import ----

        [TestCase("not-an-imdb-id")]
        [TestCase("tt123")]            // too few digits
        [TestCase("tt1234567890")]     // too many digits
        [TestCase("TT0133093")]        // the pattern is case-sensitive
        [TestCase("")]
        public async Task Import_WithMalformedImdbId_ReturnsBadRequestWithoutCallingTheImporter(string imdbId)
        {
            var result = await _sut.Import(imdbId);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _importerMock.Verify(i => i.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Import_WithNullImdbId_ReturnsBadRequest()
        {
            var result = await _sut.Import(null!);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [TestCase("tt0133093")]    // 7 digits
        [TestCase("tt12345678")]   // 8 digits
        [TestCase("tt123456789")]  // 9 digits
        public async Task Import_AcceptsSevenToNineDigitIds(string imdbId)
        {
            _importerMock
                .Setup(i => i.ImportAsync(imdbId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FilmImportResult(FilmImportStatus.NotFoundInOmdb));

            await _sut.Import(imdbId);

            _importerMock.Verify(i => i.ImportAsync(imdbId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Import_WhenImported_ReturnsCreatedAtGetById()
        {
            var imported = new FilmImportResult(FilmImportStatus.Imported, new FilmDto { Id = 31 });
            _importerMock.Setup(i => i.ImportAsync(ValidImdbId, It.IsAny<CancellationToken>())).ReturnsAsync(imported);

            var result = await _sut.Import(ValidImdbId);

            var created = (CreatedAtActionResult)result;
            Assert.That(created.ActionName, Is.EqualTo(nameof(FilmsController.GetById)));
            Assert.That(created.RouteValues!["id"], Is.EqualTo(31));
            Assert.That(created.Value, Is.SameAs(imported));
        }

        [Test]
        public async Task Import_WhenAlreadyImported_ReturnsConflict()
        {
            _importerMock
                .Setup(i => i.ImportAsync(ValidImdbId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FilmImportResult(FilmImportStatus.AlreadyImported, new FilmDto { Id = 2 }));

            var result = await _sut.Import(ValidImdbId);

            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task Import_WhenNotAMovie_ReturnsUnprocessableEntity()
        {
            _importerMock
                .Setup(i => i.ImportAsync(ValidImdbId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FilmImportResult(FilmImportStatus.NotAMovie));

            var result = await _sut.Import(ValidImdbId);

            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task Import_WhenOmdbHasNoSuchFilm_ReturnsNotFound()
        {
            _importerMock
                .Setup(i => i.ImportAsync(ValidImdbId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FilmImportResult(FilmImportStatus.NotFoundInOmdb));

            var result = await _sut.Import(ValidImdbId);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Import_WhenOmdbIsUnreachable_ReturnsBadGateway()
        {
            _importerMock
                .Setup(i => i.ImportAsync(ValidImdbId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("network down"));

            var result = await _sut.Import(ValidImdbId);

            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status502BadGateway));
        }
    }
}
