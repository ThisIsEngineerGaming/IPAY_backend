using IPAY.Application.DTOs.Media;
using IPAY.Application.Services.Media;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    /// <summary>The failure branches of OmdbController that OmdbControllerTests does not reach.</summary>
    [TestFixture]
    public class OmdbControllerErrorHandlingTests
    {
        private Mock<IOmdbService> _omdbMock = null!;
        private OmdbController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _omdbMock = new Mock<IOmdbService>();
            _sut = new OmdbController(_omdbMock.Object);
        }

        private static string? Message(object? body) =>
            body?.GetType().GetProperty("message")?.GetValue(body) as string;

        // ---- Search ----

        [Test]
        public async Task Search_WhenOmdbGivesNoErrorText_UsesTheDefaultNotFoundMessage()
        {
            _omdbMock
                .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbSearchResponseDto { Response = "false", Error = null });

            var result = (NotFoundObjectResult)await _sut.Search("nothing");

            Assert.That(Message(result.Value), Is.EqualTo("No results found."));
        }

        [Test]
        public async Task Search_WhenOmdbExplainsTheError_PassesTheMessageOn()
        {
            _omdbMock
                .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbSearchResponseDto { Response = "False", Error = "Too many results." });

            var result = (NotFoundObjectResult)await _sut.Search("a");

            Assert.That(Message(result.Value), Is.EqualTo("Too many results."));
        }

        [Test]
        public async Task Search_ForwardsTheRequestedPage()
        {
            _omdbMock
                .Setup(s => s.SearchAsync("matrix", 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbSearchResponseDto { Response = "True" });

            var result = await _sut.Search("matrix", 3);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _omdbMock.Verify(s => s.SearchAsync("matrix", 3, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Search_WhenOmdbTimesOut_ReturnsGatewayTimeout()
        {
            _omdbMock
                .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("timeout")); // the caller did not cancel, so this is a timeout

            var result = await _sut.Search("matrix");

            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status504GatewayTimeout));
        }

        [Test]
        public async Task Search_WhenTheCallerCancels_TheCancellationPropagates()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            _omdbMock
                .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            Assert.ThrowsAsync<OperationCanceledException>(() => _sut.Search("matrix", 1, cts.Token));
            await Task.CompletedTask;
        }

        [Test]
        public async Task Search_WhenSomethingUnexpectedBreaks_ReturnsInternalServerErrorWithDetail()
        {
            _omdbMock
                .Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("bad json"));

            var result = (ObjectResult)await _sut.Search("matrix");

            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(result.Value!.GetType().GetProperty("detail")!.GetValue(result.Value), Is.EqualTo("bad json"));
        }

        // ---- GetByImdbId ----

        [Test]
        public async Task GetByImdbId_WhenOmdbGivesNoErrorText_UsesTheDefaultNotFoundMessage()
        {
            _omdbMock
                .Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OmdbFilmDto { Response = "False", Error = null });

            var result = (NotFoundObjectResult)await _sut.GetByImdbId("tt0000001");

            Assert.That(Message(result.Value), Is.EqualTo("Film not found."));
        }

        [Test]
        public async Task GetByImdbId_WhenOmdbTimesOut_ReturnsGatewayTimeout()
        {
            _omdbMock
                .Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("timeout"));

            var result = await _sut.GetByImdbId("tt0133093");

            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status504GatewayTimeout));
        }

        [Test]
        public async Task GetByImdbId_WhenTheCallerCancels_TheCancellationPropagates()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            _omdbMock
                .Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            Assert.ThrowsAsync<OperationCanceledException>(() => _sut.GetByImdbId("tt0133093", cts.Token));
            await Task.CompletedTask;
        }

        [Test]
        public async Task GetByImdbId_WhenSomethingUnexpectedBreaks_ReturnsInternalServerError()
        {
            _omdbMock
                .Setup(s => s.GetByImdbIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("bad json"));

            var result = await _sut.GetByImdbId("tt0133093");

            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }
    }
}
