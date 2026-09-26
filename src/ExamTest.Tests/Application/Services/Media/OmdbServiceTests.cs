using System.Net;
using System.Net.Http.Json;
using System.Threading;
using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Services.Media;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Media
{
    [TestFixture]
    public class OmdbServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
        private HttpClient _httpClient = null!;
        private OmdbService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _sut = new OmdbService(_httpClient, "test-api-key", "https://www.omdbapi.com");
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        private void SetupResponse(object body)
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(body)
                });
        }

        [Test]
        public async Task SearchAsync_WithValidQuery_ReturnsSearchResponse()
        {
            SetupResponse(new OmdbSearchResponseDto
            {
                Response = "True",
                TotalResults = "1",
                Search = new List<OmdbSearchItemDto> { new() { Title = "Matrix", ImdbId = "tt0133093" } }
            });

            var result = await _sut.SearchAsync("matrix");

            Assert.That(result.Response, Is.EqualTo("True"));
            Assert.That(result.Search, Has.Count.EqualTo(1));
            Assert.That(result.Search![0].Title, Is.EqualTo("Matrix"));
        }

        [Test]
        public void SearchAsync_WithEmptyQuery_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _sut.SearchAsync("   "));
        }

        [Test]
        public async Task SearchAsync_WhenOmdbReturnsFalseResponse_ReturnsErrorDto()
        {
            SetupResponse(new OmdbSearchResponseDto
            {
                Response = "False",
                Error = "Movie not found!"
            });

            var result = await _sut.SearchAsync("doesnotexist");

            Assert.That(result.Response, Is.EqualTo("False"));
            Assert.That(result.Error, Is.EqualTo("Movie not found!"));
        }

        [Test]
        public async Task SearchAsync_ClampsPageOutOfRange()
        {
            HttpRequestMessage? capturedRequest = null;
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new OmdbSearchResponseDto { Response = "True" })
                });

            await _sut.SearchAsync("matrix", page: 999);

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest!.RequestUri!.Query, Does.Contain("page=100"));
        }

        [Test]
        public async Task GetByImdbIdAsync_WithValidId_ReturnsFilmDto()
        {
            SetupResponse(new OmdbFilmDto
            {
                Title = "Matrix",
                ImdbId = "tt0133093",
                Response = "True"
            });

            var result = await _sut.GetByImdbIdAsync("tt0133093");

            Assert.That(result.Title, Is.EqualTo("Matrix"));
            Assert.That(result.Response, Is.EqualTo("True"));
        }

        [Test]
        public void GetByImdbIdAsync_WithEmptyId_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _sut.GetByImdbIdAsync(""));
        }

        [Test]
        public async Task GetByImdbIdAsync_WhenOmdbReturnsFalseResponse_ReturnsErrorDto()
        {
            SetupResponse(new OmdbFilmDto
            {
                Response = "False",
                Error = "Incorrect IMDb ID."
            });

            var result = await _sut.GetByImdbIdAsync("bad-id");

            Assert.That(result.Response, Is.EqualTo("False"));
            Assert.That(result.Error, Is.EqualTo("Incorrect IMDb ID."));
        }
    }
}
