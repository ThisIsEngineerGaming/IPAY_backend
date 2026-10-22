using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IPAY.Infrastructure.Cloudinary;

public class CloudinaryImageService
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private const int MaxImagesPerRequest = 10;
    private const int MaxParallelUploads = 3;

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

    private void ValidateImage(IFormFile file)
    {
        if (file.Length == 0)
            throw new ArgumentException($"File '{file.FileName}' is empty.");
        if (file.Length > MaxImageSizeBytes)
            throw new ArgumentException($"'{file.FileName}': images must be 5 MB or smaller.");
        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new ArgumentException($"'{file.FileName}': only JPEG, PNG, GIF, and WebP images are supported.");
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

    public async Task<IReadOnlyList<string>> UploadImagesAsync(
    IEnumerable<IFormFile> files,
    CancellationToken cancellationToken)
    {
        if (_cloudinary is null)
        {
            throw new InvalidOperationException(
                "Image hosting is not configured on the server: set Cloudinary:CloudName, Cloudinary:ApiKey and " +
                "Cloudinary:ApiSecret (on Render: Cloudinary__CloudName, Cloudinary__ApiKey, Cloudinary__ApiSecret).");
        }

        var list = files?.ToList() ?? new List<IFormFile>();

        if (list.Count == 0)
            throw new ArgumentException("Choose at least one image file.");
        if (list.Count > MaxImagesPerRequest)
            throw new ArgumentException($"You can upload up to {MaxImagesPerRequest} images at once.");

        // Сначала проверяем ВСЕ файлы, чтобы не загрузить часть и потом упасть на ошибке
        foreach (var file in list)
            ValidateImage(file);

        // Загружаем параллельно, но не больше MaxParallelUploads одновременно
        using var gate = new SemaphoreSlim(MaxParallelUploads);

        var tasks = list.Select(async file =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                // Вызываем ваш существующий метод для одного файла.
                // Он вернёт то же, что и раньше (URL загруженной картинки).
                return await UploadImageAsync(file, cancellationToken);
            }
            finally
            {
                gate.Release();
            }
        });

        // Результаты возвращаются в том же порядке, что и файлы
        var urls = await Task.WhenAll(tasks);
        return urls;
    }


}
