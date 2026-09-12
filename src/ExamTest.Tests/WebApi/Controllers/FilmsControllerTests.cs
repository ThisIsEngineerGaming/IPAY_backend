using ExamTest.Application.Services.Media;
using ExamTest.WebApi.Controllers;
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
        }

        [Test]
        public async Task Search_WithValidQuery_ReturnsOk()
        {
        }

        [Test]
        public async Task Search_WhenServiceReturnsFalseResponse_ReturnsNotFound()
        {
        }

        [Test]
        public async Task Search_WhenServiceThrowsHttpRequestException_ReturnsBadGateway()
        {
        }

        [Test]
        public async Task GetByImdbId_WithValidId_ReturnsOk()
        {
        }

        [Test]
        public async Task GetByImdbId_WhenServiceReturnsFalseResponse_ReturnsNotFound()
        {
        }

        [Test]
        public async Task GetByImdbId_WhenServiceThrowsHttpRequestException_ReturnsBadGateway()
        {
        }
    }
}
