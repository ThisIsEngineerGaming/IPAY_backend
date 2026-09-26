using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Interfaces.Shop;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoryDto>> GetAll() => service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } category ? Ok(category) : NotFound();

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(SaveCategoryDto category)
    {
        var created = await service.CreateAsync(category);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveCategoryDto category)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateCategoryAsync(id, category);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/products/{productId:int}")]
    public async Task<IActionResult> AddProduct(int id, int productId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.AddToCategoryAsync(id, productId);
        return NoContent();
    }

    [HttpDelete("{id:int}/products/{productId:int}")]
    public async Task<IActionResult> RemoveProduct(int id, int productId)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.RemoveFromCategoryAsync(id, productId);
        return NoContent();
    }
}
