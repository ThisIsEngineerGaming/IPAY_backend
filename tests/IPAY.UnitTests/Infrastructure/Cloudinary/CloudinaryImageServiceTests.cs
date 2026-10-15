using IPAY.Infrastructure.Cloudinary;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Cloudinary
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

        // The constructor deliberately never throws on missing settings (a throw while an upload request is
        // running kills the response before CORS headers are added). The failure surfaces from UploadImageAsync.
        [TestCase("", "k", "s")]
        [TestCase("c", "", "s")]
        [TestCase("c", "k", "")]
        [TestCase("  ", "k", "s")]
        public void Constructor_WithMissingSettings_DoesNotThrow(string cloudName, string apiKey, string apiSecret)
        {
            var options = Options.Create(new CloudinaryOptions { CloudName = cloudName, ApiKey = apiKey, ApiSecret = apiSecret });

            Assert.DoesNotThrow(() => new CloudinaryImageService(options));
        }

        [TestCase("", "k", "s")]
        [TestCase("c", "", "s")]
        [TestCase("c", "k", "")]
        public void UploadImageAsync_WhenNotConfigured_ThrowsInvalidOperationExceptionNamingTheSettings(
            string cloudName, string apiKey, string apiSecret)
        {
            var sut = new CloudinaryImageService(
                Options.Create(new CloudinaryOptions { CloudName = cloudName, ApiKey = apiKey, ApiSecret = apiSecret }));
            var file = CreateFormFileMock(length: 1024, contentType: "image/png");

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await sut.UploadImageAsync(file.Object, CancellationToken.None));
            Assert.That(ex!.Message, Does.Contain("Cloudinary:CloudName"));
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
