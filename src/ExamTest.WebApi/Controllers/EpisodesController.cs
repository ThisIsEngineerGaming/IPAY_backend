using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/episodes")]
public class EpisodesController(IEpisodeService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Episode>> GetAll([FromQuery] int? seriesId) =>
        seriesId.HasValue ? service.GetBySeriesIdAsync(seriesId.Value) : service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Episode>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } episode ? Ok(episode) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Episode>> Create(Episode episode)
    {
        var created = await service.CreateAsync(episode);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Episode episode)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, episode);
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
