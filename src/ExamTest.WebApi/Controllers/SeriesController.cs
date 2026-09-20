using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/series")]
public class SeriesController(ISeriesService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<SeriesDto>> GetAll() => service.GetAllAsync();

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
