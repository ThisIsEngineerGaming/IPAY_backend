using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Interfaces.Shop;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/manufacturers")]
public class ManufacturersController(IManufacturerService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ManufacturerDto>> GetAll() => service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ManufacturerDto>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } manufacturer ? Ok(manufacturer) : NotFound();

    [HttpPost]
    public async Task<ActionResult<ManufacturerDto>> Create(SaveManufacturerDto manufacturer)
    {
        var created = await service.CreateAsync(manufacturer);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveManufacturerDto manufacturer)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, manufacturer);
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
