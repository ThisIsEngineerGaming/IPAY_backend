using ExamTest.Application.Interfaces.Shop;
using ExamTest.Domain.Entities.Shop;
using Microsoft.AspNetCore.Mvc;

namespace ExamTest.WebApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<Product>> GetAll() => service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } product ? Ok(product) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        var created = await service.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, product);
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
