using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ExamTest.Infastructure.Cloudinary
{
    public class CloudinaryImageService
    {
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp"
        };

        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly string _folder;

        public CloudinaryImageService(IOptions<CloudinaryOptions> options)
        {
            var settings = options.Value;
            if (string.IsNullOrWhiteSpace(settings.CloudName) ||
                string.IsNullOrWhiteSpace(settings.ApiKey) ||
                string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary:CloudName, ApiKey and ApiSecret must all be configured.");
            }

            _cloudinary = new CloudinaryDotNet.Cloudinary(
                new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
            _folder = settings.Folder;
        }

        /// <summary>Uploads the image and returns its public HTTPS URL.</summary>
        public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            if (file.Length == 0) throw new ArgumentException("Choose an image file.");
            if (file.Length > MaxImageSizeBytes) throw new ArgumentException("Images must be 5 MB or smaller.");
            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                throw new ArgumentException("Only JPEG, PNG, GIF, and WebP images are supported.");
            }

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = _folder,
                PublicId = Guid.NewGuid().ToString("N"),
                UseFilename = false,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (result.Error is not null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
            }

            // Prefer SecureUrl (https) - Cloudinary serves the image directly from its own CDN,
            // so unlike the Firebase Storage version there's no need for our own download/proxy endpoint.
            return result.SecureUrl?.ToString() ?? result.Url.ToString();
        }

        /// <summary>Deletes an image by the public ID you got back embedded in its URL.</summary>
        public async Task DeleteImageAsync(string publicId, CancellationToken cancellationToken)
        {
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
