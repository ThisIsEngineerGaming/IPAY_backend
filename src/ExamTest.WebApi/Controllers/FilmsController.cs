
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;

using ExamTest.Application.Services.Media;

using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/films")]
public sealed class FilmsController : ControllerBase
{
    private readonly IOmdbService _omdbService;

    public FilmsController(IOmdbService omdbService)
    {
        _omdbService = omdbService;
    }

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
            var result = await _omdbService.SearchAsync(query, page, cancellationToken);
            if (string.Equals(result.Response, "False", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Error ?? "No results found." });

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not reach OMDb.", detail = ex.Message });
        }
    }

    [HttpGet("{imdbId}")]
    public async Task<IActionResult> GetByImdbId(
        string imdbId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _omdbService.GetByImdbIdAsync(imdbId, cancellationToken);
            if (string.Equals(result.Response, "False", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = result.Error ?? "Film not found." });

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not reach OMDb.", detail = ex.Message });
        }
    }
}
