using ExamTest.Infastructure.Cloudinary;
using ExamTest.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    [TestFixture]
    public class ImagesControllerTests
    {
        // CloudinaryImageService wraps the sealed, non-virtual CloudinaryDotNet.Cloudinary
        // client, so it can't be mocked with Moq. It's safe to use a real instance here
        // though: every case below is rejected by UploadImageAsync's own guard clauses
        // (empty file / too large / wrong content type) before it ever reaches the network,
        // so no live Cloudinary credentials are required. The happy path that actually
        // calls Cloudinary is out of scope for a unit test and would belong in an
        // integration suite instead.
        private static CloudinaryImageService CreateStorage() => new(Options.Create(new CloudinaryOptions
        {
            CloudName = "demo-cloud",
            ApiKey = "demo-key",
            ApiSecret = "demo-secret"
        }));

        private static Mock<IFormFile> CreateFormFileMock(long length, string contentType, string fileName = "photo.png")
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.SetupGet(f => f.Length).Returns(length);
            fileMock.SetupGet(f => f.ContentType).Returns(contentType);
            fileMock.SetupGet(f => f.FileName).Returns(fileName);
            return fileMock;
        }

        private static ImagesController CreateController() =>
            new(CreateStorage(), new Mock<ILogger<ImagesController>>().Object);

        [Test]
        public async Task Upload_WithEmptyFile_ReturnsBadRequest()
        {
            var sut = CreateController();
            var file = CreateFormFileMock(length: 0, contentType: "image/png");

            var result = await sut.Upload(file.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Upload_WithFileTooLarge_ReturnsBadRequest()
        {
            var sut = CreateController();
            var file = CreateFormFileMock(length: 10 * 1024 * 1024, contentType: "image/png");

            var result = await sut.Upload(file.Object, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Upload_WithUnsupportedContentType_ReturnsBadRequest()
        {
            var sut = CreateController();
            var file = CreateFormFileMock(length: 1024, contentType: "application/pdf");

            var result = await sut.Upload(file.Object, CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }
    }
}
