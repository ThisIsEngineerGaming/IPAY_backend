using ExamTest.Infastructure.Cloudinary;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController(CloudinaryImageService storage, ILogger<ImagesController> logger) : ControllerBase
{
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
}
