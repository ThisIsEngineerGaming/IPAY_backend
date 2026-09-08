using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(IGenreService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Genre>> GetAll() => service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Genre>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } genre ? Ok(genre) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Genre>> Create(Genre genre)
    {
        var created = await service.CreateAsync(genre);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Genre genre)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, genre);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/series/{seriesId:int}")]
    public async Task<IActionResult> AddSeries(int id, int seriesId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.AddSeriesToGenreAsync(id, seriesId);
        return NoContent();
    }

    [HttpDelete("{id:int}/series/{seriesId:int}")]
    public async Task<IActionResult> RemoveSeries(int id, int seriesId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.RemoveSeriesFromGenreAsync(id, seriesId);
        return NoContent();
    }

    [HttpPut("{id:int}/films/{filmId:int}")]
    public async Task<IActionResult> AddFilm(int id, int filmId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.AddFilmToGenreAsync(id, filmId);
        return NoContent();
    }

    [HttpDelete("{id:int}/films/{filmId:int}")]
    public async Task<IActionResult> RemoveFilm(int id, int filmId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.RemoveFilmFromGenreAsync(id, filmId);
        return NoContent();
    }
}
