using System.Text.RegularExpressions;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using IPAY.Application.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers;

[ApiController]
[Route("api/films")]
public sealed partial class FilmsController(
    IFilmService films,
    IFilmImportService importer,
    IOmdbService omdbService) : ControllerBase
{
    [GeneratedRegex(@"^tt\d{7,9}$")]
    private static partial Regex ImdbIdPattern();

    // ---- Our catalog (Firestore) ----

    /// Cursor-paged list. Pass the id of the last film you received as lastDocId to get the next page.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FilmDto>>> GetPage(
        [FromQuery] int limit = 20,
        [FromQuery] string? lastDocId = null,
        [FromQuery] int? genreId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null) =>
        Ok(await films.GetPageAsync(limit, lastDocId, genreId, search, sortBy, sortDir));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FilmDto>> GetById(int id) =>
        await films.GetByIdAsync(id) is { } film ? Ok(film) : NotFound();

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<FilmDto>> Create(SaveFilmDto film)
    {
        var created = await films.CreateAsync(film);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveFilmDto film)
    {
        if (await films.GetByIdAsync(id) is null) return NotFound();
        await films.UpdateAsync(id, film);
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await films.GetByIdAsync(id) is null) return NotFound();
        await films.DeleteAsync(id);
        return NoContent();
    }

    /// Copies a film from OMDb into our catalog. Matching genres are linked; missing genres are reported, never created.
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("import/{imdbId}")]
    public async Task<IActionResult> Import(string imdbId, CancellationToken cancellationToken = default)
    {
        if (!ImdbIdPattern().IsMatch(imdbId ?? string.Empty))
            return BadRequest(new { message = "imdbId must look like tt0133093." });

        try
        {
            var result = await importer.ImportAsync(imdbId!, cancellationToken);
            return result.Status switch
            {
                FilmImportStatus.Imported =>
                    CreatedAtAction(nameof(GetById), new { id = result.Film!.Id }, result),
                FilmImportStatus.AlreadyImported =>
                    Conflict(new { message = "This film has already been imported.", film = result.Film }),
                FilmImportStatus.NotAMovie =>
                    UnprocessableEntity(new { message = "That IMDb id is not a movie." }),
                _ => NotFound(new { message = "Film not found in OMDb." })
            };
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not reach OMDb.", detail = ex.Message });
        }
    }

    // ---- OMDb lookups (previously api/films/search and api/films/{imdbId}) ----

    [HttpGet("omdb/search")]
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
    }

    [HttpGet("omdb/{imdbId}")]
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
    }
}
