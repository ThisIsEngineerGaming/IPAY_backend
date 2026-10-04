using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IPAY.WebApi.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController(ICartService service) : ControllerBase
    {
        [Authorize(Policy = "AnyAuthenticated")]
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetMyCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);//userId
            var cart = await service.GetByUserIdAsync(userId);
            if (cart == null)
            {
                return Ok(new CartDto());
            }
            else
            {
                return Ok(cart);
            }
           
        }

        [Authorize(Policy = "AnyAuthenticated")]
        [HttpPost("items")]
        public async Task<ActionResult<CartDto>> AddItem(CreateCartItemDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cart = await service.AddItemAsync(userId, dto);
            return Ok(cart);
        }

        [Authorize(Policy = "AnyAuthenticated")]
        [HttpPatch("items/{itemId:int}")]
        public async Task<ActionResult<CartDto>> UpdateItemQuantity(int itemId, CreateCartItemDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cart = await service.UpdateItemQuantityAsync(userId, itemId, dto);
            if (cart == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(cart);
            }
           
        }

        [Authorize(Policy = "AnyAuthenticated")]
        [HttpDelete("items/{itemId:int}")]
        public async Task<ActionResult<CartDto>> RemoveItem(int itemId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cart = await service.RemoveItemAsync(userId, itemId);
            if (cart == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(cart);
            }
        }

        [Authorize(Policy = "AnyAuthenticated")]
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await service.ClearCartAsync(userId);
            return NoContent();
        }
    }
}
