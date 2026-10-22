using IPAY.Infrastructure.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers;

[ApiController]
[Route("api/images")]

public class UploadImagesRequest
{
    public List<IFormFile> Files { get; set; } = new();
}
public class ImagesController(CloudinaryImageService storage, ILogger<ImagesController> logger) : ControllerBase
{
    [Authorize(Roles = "Seller,Admin,Moderator")]
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<object>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            // Cloudinary serves images straight from its own CDN, so this is already the
            // final, directly-usable HTTPS URL - unlike the old Firebase setup there's no
            // separate /api/images/{objectName} proxy endpoint needed.
            var url = await storage.UploadImageAsync(file, cancellationToken);
            return Ok(new { url });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (Exception exception)
        {
            // Catches Cloudinary API errors (bad credentials, quota, network, etc.) that
            // aren't ArgumentException, so the client gets readable JSON back instead of a
            // raw, unhandled-exception response it can't parse.
            logger.LogError(exception, "Image upload failed.");
            return StatusCode(500, new { error = exception.Message });
        }
    }
    // Метод внутри вашего контроллера (подставьте свой сервис вместо _imageService)
    [HttpPost("upload-many")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> UploadMany(
        [FromForm] UploadImagesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var urls = await storage.UploadImagesAsync(request.Files, cancellationToken);
            return Ok(urls);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}


