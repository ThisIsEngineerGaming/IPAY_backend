using IPAY.Application.Interfaces.Media;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    // The OMDb lookups (api/films/omdb/...) are tested in OmdbControllerTests.
    [TestFixture]
    public class FilmsControllerTests
    {
        private Mock<IFilmImportService> _importerMock = null!;
        private FilmsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _importerMock = new Mock<IFilmImportService>();
            _sut = new FilmsController(
                new Mock<IFilmService>().Object,
                _importerMock.Object);
        }

        [Test]
        public async Task Import_WithMalformedImdbId_ReturnsBadRequest()
        {
            var result = await _sut.Import("not-an-imdb-id");

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _importerMock.Verify(i => i.ImportAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
