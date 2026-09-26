using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class FilmsControllerTests
    {
        private Mock<IOmdbService> _omdbServiceMock = null!;
        private FilmsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _omdbServiceMock = new Mock<IOmdbService>();
            _sut = new FilmsController(_omdbServiceMock.Object);
        }

        [Test]
        public async Task Search_WithMissingQuery_ReturnsBadRequest()
        {
            var result = await _sut.Search(query: "");

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _omdbServiceMock.Verify(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Search_WithValidQuery_ReturnsOk()
        {
            _omdbServiceMock
                .Setup(s => s.SearchAsync("matrix", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbSearchResponseDto { Response = "True" });

            var result = await _sut.Search(query: "matrix");

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(((OmdbSearchResponseDto)okResult!.Value!).Response, Is.EqualTo("True"));
        }

        [Test]
        public async Task Search_WhenServiceReturnsFalseResponse_ReturnsNotFound()
        {
            _omdbServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbSearchResponseDto { Response = "False", Error = "Movie not found!" });

            var result = await _sut.Search(query: "doesnotexist");

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Search_WhenServiceThrowsHttpRequestException_ReturnsBadGateway()
        {
            _omdbServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("network down"));

            var result = await _sut.Search(query: "matrix");

            var statusResult = result as ObjectResult;
            Assert.That(statusResult, Is.Not.Null);
            Assert.That(statusResult!.StatusCode, Is.EqualTo(StatusCodes.Status502BadGateway));
        }

        [Test]
        public async Task GetByImdbId_WithValidId_ReturnsOk()
        {
            _omdbServiceMock
                .Setup(s => s.GetByImdbIdAsync("tt0133093", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbFilmDto { Response = "True", Title = "Matrix" });

            var result = await _sut.GetByImdbId("tt0133093");

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(((OmdbFilmDto)okResult!.Value!).Title, Is.EqualTo("Matrix"));
        }

        [Test]
        public async Task GetByImdbId_WhenServiceReturnsFalseResponse_ReturnsNotFound()
        {
            _omdbServiceMock
                .Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbFilmDto { Response = "False", Error = "Incorrect IMDb ID." });

            var result = await _sut.GetByImdbId("bad-id");

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetByImdbId_WhenServiceThrowsHttpRequestException_ReturnsBadGateway()
        {
            _omdbServiceMock
                .Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("network down"));

            var result = await _sut.GetByImdbId("tt0133093");

            var statusResult = result as ObjectResult;
            Assert.That(statusResult, Is.Not.Null);
            Assert.That(statusResult!.StatusCode, Is.EqualTo(StatusCodes.Status502BadGateway));
        }
    }
}
