

using System.Security.Claims;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Отзывы по товару (можно без авторизации — по желанию убери AllowAnonymous)
        /// </summary>
        [HttpGet("product/{productId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetByProduct(int productId)
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId);
            return Ok(reviews);
        }

        /// <summary>
        /// Один отзыв по id
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewDto>> GetById(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review is null)
                return NotFound($"Review with id {id} not found");

            return Ok(review);
        }

        /// <summary>
        /// Оставить отзыв
        /// </summary>
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewDto dto)
        {
            var userId = GetUserId();
            if (userId is null)
                return Unauthorized("User id not found in token");

            var review = await _reviewService.CreateAsync(dto, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        /// <summary>
        /// Удалить отзыв (автор или админ)
        /// </summary>
        [Authorize(Policy = "NotBanned")]
        [Authorize(Roles = "Admin,Moderator,Customer,Seller")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (userId is null)
                return Unauthorized("User id not found in token");

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

            try
            {
                await _reviewService.DeleteAsync(id, userId.Value, isAdmin);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
