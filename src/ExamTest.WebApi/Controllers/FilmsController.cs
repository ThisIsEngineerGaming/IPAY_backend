<<<<<<< HEAD
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
=======
using ExamTest.Application.Services.Media;
>>>>>>> master
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/films")]
<<<<<<< HEAD
public class FilmsController(IFilmService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Film>> GetAll() => service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Film>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } film ? Ok(film) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Film>> Create(Film film)
    {
        var created = await service.CreateAsync(film);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Film film)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, film);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.DeleteAsync(id);
        return NoContent();
=======
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
>>>>>>> master
    }
}
