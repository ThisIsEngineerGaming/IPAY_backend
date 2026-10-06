using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ==================== ORDER ====================
        [Authorize(Policy ="NotBanned")]
        [Authorize(Roles ="Admin,Moderator,Customer,Seller")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order is null)
                return NotFound($"Order with id {id} not found");

            return Ok(order);
        }
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetByUserId(int userId)
        {
            var orders = await _orderService.GetByUserIdAsync(userId);
            return Ok(orders);
        }
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
        {
            // В реальном проекте UserId берётся из токена:
            // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userId = 1; // временно для курсовой

            var order = await _orderService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            await _orderService.UpdateStatusAsync(id, dto);
            return NoContent();
        }
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return NoContent();
        }

        // ==================== ORDER ITEMS ====================
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpPost("{orderId:int}/items")]
        public async Task<ActionResult<OrderItemDto>> AddItem(int orderId, [FromBody] CreateOrderItemDto dto)
        {
            var item = await _orderService.AddItemAsync(orderId, dto);
            return Ok(item);
        }
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpPut("{orderId:int}/items/{itemId:int}")]
        public async Task<IActionResult> UpdateItem(int orderId, int itemId, [FromBody] CreateOrderItemDto dto)
        {
            await _orderService.UpdateItemAsync(orderId, itemId, dto);
            return NoContent();
        }
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpDelete("{orderId:int}/items/{itemId:int}")]
        public async Task<IActionResult> DeleteItem(int orderId, int itemId)
        {
            await _orderService.DeleteItemAsync(orderId, itemId);
            return NoContent();
        }
    }
}
