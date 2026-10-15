using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers;

[ApiController]
[Route("api/series")]
[ServiceFilter(typeof(IPAY.WebApi.Filters.ValidationFilter))]
public class SeriesController(ISeriesService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<SeriesDto>> GetAll() => service.GetAllAsync();

    /// Cursor-paged list (same query options as GET api/films). Pass the last id you received as lastDocId.
    [HttpGet("page")]
    public async Task<ActionResult<IReadOnlyList<SeriesDto>>> GetPage(
        [FromQuery] int limit = 20,
        [FromQuery] string? lastDocId = null,
        [FromQuery] int? genreId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null) =>
        Ok(await service.GetPageAsync(limit, lastDocId, genreId, search, sortBy, sortDir));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SeriesDto>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } series ? Ok(series) : NotFound();

    [HttpPost]
    public async Task<ActionResult<SeriesDto>> Create(SaveSeriesDto series)
    {
        var created = await service.CreateAsync(series);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveSeriesDto series)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, series);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.DeleteAsync(id);
        return NoContent();
    }
}
