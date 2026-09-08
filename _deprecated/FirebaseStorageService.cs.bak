using Google;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ExamTest.Infastructure.Firebase;

public class FirebaseStorageService(StorageClient storageClient, IOptions<FirebaseOptions> options)
{
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string> Extensions = new Dictionary<string, string>
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/gif"] = ".gif",
        ["image/webp"] = ".webp"
    };

    private readonly string _bucket = options.Value.StorageBucket;

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0) throw new ArgumentException("Choose an image file.");
        if (file.Length > MaxImageSizeBytes) throw new ArgumentException("Images must be 5 MB or smaller.");
        if (!Extensions.TryGetValue(file.ContentType.ToLowerInvariant(), out var extension))
        {
            throw new ArgumentException("Only JPEG, PNG, GIF, and WebP images are supported.");
        }
        if (string.IsNullOrWhiteSpace(_bucket)) throw new InvalidOperationException("Firebase:StorageBucket must be configured.");

        var objectName = $"images/{Guid.NewGuid():N}{extension}";
        await using var stream = file.OpenReadStream();
        await storageClient.UploadObjectAsync(_bucket, objectName, file.ContentType, stream, cancellationToken: cancellationToken);
        return objectName;
    }

    public async Task<(Stream Content, string ContentType)?> DownloadImageAsync(string objectName, CancellationToken cancellationToken)
    {
        if (!objectName.StartsWith("images/", StringComparison.Ordinal) || objectName.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            var metadata = await storageClient.GetObjectAsync(_bucket, objectName, cancellationToken: cancellationToken);
            var stream = new MemoryStream();
            await storageClient.DownloadObjectAsync(_bucket, objectName, stream, cancellationToken: cancellationToken);
            stream.Position = 0;
            return (stream, metadata.ContentType ?? "application/octet-stream");
        }
        catch (GoogleApiException exception) when (exception.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
