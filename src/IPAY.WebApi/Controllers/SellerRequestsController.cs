using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IPAY.WebApi.Controllers
{
    /// <summary>
    /// Customers ask to become sellers; admins and moderators review the requests. The requester's id always
    /// comes from the JWT. Approving changes the customer's role, which signs them out (see the role check in
    /// Program.cs), so they sign back in as a seller.
    /// </summary>
    [ApiController]
    [Route("api/seller-requests")]
    public class SellerRequestsController(ISellerRequestService service) : ControllerBase
    {
        // --- the requester ---

        [Authorize(Policy = "CustomerOnly")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSellerRequestDto? dto)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            return ToActionResult(await service.CreateAsync(userId, dto?.Message));
        }

        /// <summary>The caller's latest request (204 if they never made one).</summary>
        [Authorize(Roles = "Customer,Seller")]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var latest = await service.GetLatestForUserAsync(userId);
            return latest is null ? NoContent() : Ok(latest);
        }

        // --- staff ---

        [Authorize(Policy = "ModeratorOrAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

        [Authorize(Policy = "ModeratorOrAdmin")]
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id) =>
            ToActionResult(await service.ApproveAsync(id, Reviewer()));

        [Authorize(Policy = "ModeratorOrAdmin")]
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id) =>
            ToActionResult(await service.RejectAsync(id, Reviewer()));

        // --- helpers ---

        private bool TryGetUserId(out int userId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        private string Reviewer() =>
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "staff";

        private IActionResult ToActionResult(SellerRequestResult result)
        {
            if (result.NotFound) return NotFound(result.Error);
            if (result.Error is not null) return BadRequest(result.Error);
            return Ok(result.Request);
        }
    }
}
