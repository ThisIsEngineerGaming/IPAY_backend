using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IPAY.WebApi.Controllers
{
    [ApiController]
    [Route("api/cart-items")]
    [Authorize(Policy = "AnyAuthenticated")]
    public class CartItemController(ICartItemService service) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CartItemDto>> GetById(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var item = await service.GetByIdAsync(userId, id);
            if (item == null)
            {
               return  Forbid();
            }
            else
            {
                return Ok(item);
            }
          
        }

        [HttpPost]
        public async Task<ActionResult<CartItemDto>> Create(CreateCartItemDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var created = await service.AddAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CartItemDto>> Update(int id, CreateCartItemDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updated = await service.UpdateAsync(userId, id, dto);
            if (updated == null)
            {
                return Forbid();
            }
            else
            {
                return Ok(updated);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await service.DeleteAsync(userId, id);
            if (success)
            {
                return NoContent();
            }
            else
            {
                return Forbid();
            }
          
        }
    }
}
