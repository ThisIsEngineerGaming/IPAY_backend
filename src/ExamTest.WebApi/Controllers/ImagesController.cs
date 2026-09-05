using ExamTest.Infastructure.Firebase;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController(FirebaseStorageService storage, ILogger<ImagesController> logger) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<object>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var objectName = await storage.UploadImageAsync(file, cancellationToken);
            return Ok(new { url = $"/api/images/{objectName}" });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (Exception exception)
        {
            // Catches Google Cloud Storage errors (missing bucket, permission denied, API not
            // enabled, etc.) that aren't ArgumentException, so the client gets readable JSON
            // back instead of a raw, unhandled-exception response it can't parse.
            logger.LogError(exception, "Image upload failed.");
            return StatusCode(500, new { error = exception.Message });
        }
    }

    [HttpGet("{**objectName}")]
    public async Task<IActionResult> Download(string objectName, CancellationToken cancellationToken)
    {
        var image = await storage.DownloadImageAsync(objectName, cancellationToken);
        return image is null ? NotFound() : File(image.Value.Content, image.Value.ContentType);
    }
}
