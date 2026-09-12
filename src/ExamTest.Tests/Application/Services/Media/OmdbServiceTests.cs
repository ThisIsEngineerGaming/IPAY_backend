using ExamTest.Application.Services.Media;
using Moq;
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

        [Test]
        public void SearchAsync_WithValidQuery_ReturnsSearchResponse()
        {
        }

        [Test]
        public void SearchAsync_WithEmptyQuery_ThrowsArgumentException()
        {
        }

        [Test]
        public void SearchAsync_WhenOmdbReturnsFalseResponse_ReturnsErrorDto()
        {
        }

        [Test]
        public void SearchAsync_ClampsPageOutOfRange()
        {
        }

        [Test]
        public void GetByImdbIdAsync_WithValidId_ReturnsFilmDto()
        {
        }

        [Test]
        public void GetByImdbIdAsync_WithEmptyId_ThrowsArgumentException()
        {
        }

        [Test]
        public void GetByImdbIdAsync_WhenOmdbReturnsFalseResponse_ReturnsErrorDto()
        {
        }
    }
}
