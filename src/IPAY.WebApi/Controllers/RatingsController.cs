using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IPAY.WebApi.Controllers
{
    /// <summary>
    /// Product ratings. Only registered (non-guest, non-banned) users can rate, once per product, and the
    /// user id always comes from the JWT. Nobody can set a rating directly - a product's Rating is the average
    /// of these.
    /// </summary>
    [ApiController]
    [Route("api/ratings")]
    [Authorize(Policy = "RegisteredUser")]
    public class RatingsController(IProductRatingService service) : ControllerBase
    {
        /// <summary>productId -> stars the current user gave, so the UI knows which products are already rated.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(await service.GetMineAsync(userId));
        }

        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Rate(int productId, [FromBody] RateProductDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (dto is null || dto.Value is < 1 or > 5)
                return BadRequest("A rating must be between 1 and 5 stars.");

            var outcome = await service.RateAsync(productId, userId, dto.Value);

            return outcome.Result switch
            {
                RateResult.Rated => Ok(new
                {
                    rating = outcome.Rating,
                    ratingCount = outcome.RatingCount,
                    myRating = dto.Value
                }),
                RateResult.ProductNotFound => NotFound("Product not found."),
                RateResult.AlreadyRated => Conflict("You have already rated this product."),
                RateResult.OwnProduct => StatusCode(StatusCodes.Status403Forbidden,
                    "You can't rate your own product."),
                _ => BadRequest()
            };
        }
    }
}
