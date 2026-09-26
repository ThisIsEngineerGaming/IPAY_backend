using ExamTest.Infastructure.Cloudinary;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.Infrastructure.Cloudinary
{
    [TestFixture]
    public class CloudinaryImageServiceTests
    {
        private static IOptions<CloudinaryOptions> ValidOptions() => Options.Create(new CloudinaryOptions
        {
            CloudName = "demo-cloud",
            ApiKey = "demo-key",
            ApiSecret = "demo-secret"
        });

        private static Mock<IFormFile> CreateFormFileMock(long length, string contentType, string fileName = "photo.png")
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.SetupGet(f => f.Length).Returns(length);
            fileMock.SetupGet(f => f.ContentType).Returns(contentType);
            fileMock.SetupGet(f => f.FileName).Returns(fileName);
            return fileMock;
        }

        [Test]
        public void Constructor_WithMissingCloudName_ThrowsInvalidOperationException()
        {
            var options = Options.Create(new CloudinaryOptions { CloudName = "", ApiKey = "k", ApiSecret = "s" });

            Assert.Throws<InvalidOperationException>(() => new CloudinaryImageService(options));
        }

        [Test]
        public void Constructor_WithMissingApiKey_ThrowsInvalidOperationException()
        {
            var options = Options.Create(new CloudinaryOptions { CloudName = "c", ApiKey = "", ApiSecret = "s" });

            Assert.Throws<InvalidOperationException>(() => new CloudinaryImageService(options));
        }

        [Test]
        public void Constructor_WithMissingApiSecret_ThrowsInvalidOperationException()
        {
            var options = Options.Create(new CloudinaryOptions { CloudName = "c", ApiKey = "k", ApiSecret = "" });

            Assert.Throws<InvalidOperationException>(() => new CloudinaryImageService(options));
        }

        [Test]
        public void Constructor_WithAllValuesConfigured_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new CloudinaryImageService(ValidOptions()));
        }

        [Test]
        public void UploadImageAsync_WithEmptyFile_ThrowsArgumentException()
        {
            var sut = new CloudinaryImageService(ValidOptions());
            var file = CreateFormFileMock(length: 0, contentType: "image/png");

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await sut.UploadImageAsync(file.Object, CancellationToken.None));
            Assert.That(ex!.Message, Does.Contain("image file"));
        }

        [Test]
        public void UploadImageAsync_WithFileLargerThanFiveMegabytes_ThrowsArgumentException()
        {
            var sut = new CloudinaryImageService(ValidOptions());
            var file = CreateFormFileMock(length: 6 * 1024 * 1024, contentType: "image/png");

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await sut.UploadImageAsync(file.Object, CancellationToken.None));
            Assert.That(ex!.Message, Does.Contain("5 MB"));
        }

        [Test]
        public void UploadImageAsync_WithUnsupportedContentType_ThrowsArgumentException()
        {
            var sut = new CloudinaryImageService(ValidOptions());
            var file = CreateFormFileMock(length: 1024, contentType: "application/pdf");

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await sut.UploadImageAsync(file.Object, CancellationToken.None));
            Assert.That(ex!.Message, Does.Contain("JPEG, PNG, GIF, and WebP"));
        }

        [Test]
        public void UploadImageAsync_ChecksFileSizeBeforeContentType()
        {
            // An oversized file with an already-invalid content type should still fail on the
            // size guard first, since that check runs before the content-type check.
            var sut = new CloudinaryImageService(ValidOptions());
            var file = CreateFormFileMock(length: 10 * 1024 * 1024, contentType: "application/pdf");

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await sut.UploadImageAsync(file.Object, CancellationToken.None));
            Assert.That(ex!.Message, Does.Contain("5 MB"));
        }
    }
}
