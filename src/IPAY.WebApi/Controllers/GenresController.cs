using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(IGenreService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<GenreDto>> GetAll() => service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GenreDto>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } genre ? Ok(genre) : NotFound();
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<GenreDto>> Create(SaveGenreDto genre)
    {
        var created = await service.CreateAsync(genre);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveGenreDto genre)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, genre);
        return NoContent();
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.DeleteAsync(id);
        return NoContent();
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}/series/{seriesId:int}")]
    public async Task<IActionResult> AddSeries(int id, int seriesId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.AddSeriesToGenreAsync(id, seriesId);
        return NoContent();
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}/series/{seriesId:int}")]
    public async Task<IActionResult> RemoveSeries(int id, int seriesId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.RemoveSeriesFromGenreAsync(id, seriesId);
        return NoContent();
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}/films/{filmId:int}")]
    public async Task<IActionResult> AddFilm(int id, int filmId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.AddFilmToGenreAsync(id, filmId);
        return NoContent();
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}/films/{filmId:int}")]
    public async Task<IActionResult> RemoveFilm(int id, int filmId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.RemoveFilmFromGenreAsync(id, filmId);
        return NoContent();
    }
}
