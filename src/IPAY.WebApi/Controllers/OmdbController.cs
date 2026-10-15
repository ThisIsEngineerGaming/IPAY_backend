using IPAY.Application.Services.Media;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers;

/// OMDb lookups (used by the admin forms to pull a film/series by name or IMDb id).
/// Kept in its own controller, on the same URLs as before (api/films/omdb/...), so it only depends on
/// IOmdbService and keeps working even when the film catalog services (IFilmService etc.) are not registered.
[ApiController]
[Route("api/films/omdb")]
public sealed class OmdbController(IOmdbService omdbService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "The query parameter is required." });

        try
        {
            var result = await omdbService.SearchAsync(query, page, cancellationToken);
            if (string.Equals(result.Response, "False", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Error ?? "No results found." });

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not reach OMDb.", detail = ex.Message });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "OMDb took too long to respond." });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "OMDb lookup failed.", detail = ex.Message });
        }
    }

    [HttpGet("{imdbId}")]
    public async Task<IActionResult> GetByImdbId(
        string imdbId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await omdbService.GetByImdbIdAsync(imdbId, cancellationToken);
            if (string.Equals(result.Response, "False", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Error ?? "Film not found." });

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not reach OMDb.", detail = ex.Message });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "OMDb took too long to respond." });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "OMDb lookup failed.", detail = ex.Message });
        }
    }
}
