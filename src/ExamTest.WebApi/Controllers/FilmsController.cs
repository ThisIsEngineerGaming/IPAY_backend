using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/films")]
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
    }
}
