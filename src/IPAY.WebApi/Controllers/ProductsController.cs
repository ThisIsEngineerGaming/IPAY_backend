using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IPAY.WebApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet("all")]
    public Task<IReadOnlyList<ProductDto>> GetAll() => service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id) =>
        await service.GetByIdAsync(id) is { } product ? Ok(product) : NotFound();
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateAdminProductDto adminProduct)
    {
        var created = await service.CreateAsync(adminProduct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, UpdateAdminProductDto adminProduct)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, adminProduct);
        return NoContent();
    }
    [Authorize(Roles = "Seller")]
    [HttpPost("/seller")]
    public async Task<ActionResult<ProductDto>> Create(CreateSellerProductDto sellerProduct)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var created = await service.CreateAsync(sellerProduct,sellerId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [Authorize(Roles = "Seller")]
    [HttpPut("seller/{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, UpdateSellerProductDto sellerProduct)
    {
        if (await service.GetByIdAsync(id) is null) return NotFound();
        await service.UpdateAsync(id, sellerProduct);
        return NoContent();
    }
    [Authorize(Roles = "Seller,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await service.GetByIdAsync(id);
        if (product is null) return NotFound();

        // Seller може видаляти лише свій товар, Admin — будь-який
        if (User.IsInRole("Seller") && !User.IsInRole("Admin"))
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.SellerId != currentUserId)
                return Forbid();
        }

        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetLimitProduct(
        [FromQuery] int limit = 12,
        [FromQuery] string? lastDocId = null)
    {
        var result = await service.GetLimitAsync(limit, lastDocId);
        return Ok(result);
    }



    [HttpGet("filter")]
    public async Task<ActionResult<ProductDto>> GetFiltered(
    [FromQuery] int? categoryId,
    [FromQuery] double? minPrice,
    [FromQuery] double? maxPrice,
    [FromQuery] string? brand,
    [FromQuery] string? search,
    [FromQuery] int limit = 12,
    [FromQuery] string? lastDocId = null)
    {
        var result = await service.GetFilteredAsync(
            limit, lastDocId, categoryId, minPrice, maxPrice, brand, search);

        return Ok(result);
    }
    [HttpGet("sort")]
    public async Task<ActionResult<ProductDto>> GetSorted(
    [FromQuery] string sortBy = "price",   // price | rating | date
    [FromQuery] string sortDir = "asc",    // asc | desc
    [FromQuery] int limit = 12,
    [FromQuery] string? lastDocId = null)
    {
        var result = await service.GetSortedAsync(limit, lastDocId, sortBy, sortDir);
        return Ok(result);
    }
}
