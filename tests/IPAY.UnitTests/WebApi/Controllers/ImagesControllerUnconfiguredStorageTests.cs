using IPAY.Infrastructure.Cloudinary;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    /// <summary>
    /// When Cloudinary credentials are missing, CloudinaryImageService does not throw from its constructor - it
    /// throws InvalidOperationException from UploadImageAsync, and the controller must turn that into a readable
    /// JSON 500 (and log it) instead of an unhandled exception. This covers the generic catch block offline.
    /// The success path needs a live Cloudinary account and is not unit-testable.
    /// </summary>
    [TestFixture]
    public class ImagesControllerUnconfiguredStorageTests
    {
        [Test]
        public async Task Upload_WhenCloudinaryIsNotConfigured_ReturnsServerErrorWithReadableMessageAndLogsIt()
        {
            var storage = new CloudinaryImageService(Options.Create(new CloudinaryOptions()));
            var logger = new Mock<ILogger<ImagesController>>();
            var sut = new ImagesController(storage, logger.Object);
            var file = new Mock<IFormFile>();
            file.SetupGet(f => f.Length).Returns(1024);
            file.SetupGet(f => f.ContentType).Returns("image/png");
            file.SetupGet(f => f.FileName).Returns("photo.png");

            var result = await sut.Upload(file.Object, CancellationToken.None);

            var error = (ObjectResult)result.Result!;
            Assert.That(error.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(error.Value!.GetType().GetProperty("error")!.GetValue(error.Value) as string,
                Does.Contain("not configured"));
            logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
