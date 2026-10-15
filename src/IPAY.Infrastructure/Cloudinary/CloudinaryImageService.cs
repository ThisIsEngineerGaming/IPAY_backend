using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IPAY.Infrastructure.Cloudinary;

public class CloudinaryImageService
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };

    private readonly CloudinaryDotNet.Cloudinary? _cloudinary;

    public CloudinaryImageService(IOptions<CloudinaryOptions> options)
    {
        var settings = options.Value;

        // Deliberately NOT thrown from the constructor: this is a singleton resolved while an upload request
        // is running, and an exception there kills the response before CORS headers are added - the browser then
        // reports a misleading "blocked by CORS policy". Fail inside UploadImageAsync instead, where
        // ImagesController turns it into a readable JSON error.
        if (string.IsNullOrWhiteSpace(settings.CloudName) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            return;
        }

        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    /// <summary>
    /// Uploads an image to Cloudinary and returns its public, CDN-served HTTPS URL.
    /// Unlike the old Firebase setup, there's no separate download/proxy step needed -
    /// the returned URL is directly usable in an &lt;img&gt; src.
    /// </summary>
    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (_cloudinary is null)
        {
            throw new InvalidOperationException(
                "Image hosting is not configured on the server: set Cloudinary:CloudName, Cloudinary:ApiKey and " +
                "Cloudinary:ApiSecret (on Render: Cloudinary__CloudName, Cloudinary__ApiKey, Cloudinary__ApiSecret).");
        }

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
            Folder = "images",
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken: cancellationToken);

        if (result.Error is not null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        return result.SecureUrl.ToString();
    }
}
