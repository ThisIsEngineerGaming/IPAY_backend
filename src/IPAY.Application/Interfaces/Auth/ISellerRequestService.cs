using IPAY.Application.DTOs.Auth;

namespace IPAY.Application.Interfaces.Auth
{
    /// <summary>Outcome of a seller-request operation: the request, or why it didn't work.</summary>
    public record SellerRequestResult(SellerRequestDto? Request, string? Error = null, bool NotFound = false);

    public interface ISellerRequestService
    {
        /// <summary>A customer asks to become a seller. One pending request at a time.</summary>
        Task<SellerRequestResult> CreateAsync(int userId, string? message);

        /// <summary>The user's most recent request, or null if they never made one.</summary>
        Task<SellerRequestDto?> GetLatestForUserAsync(int userId);

        /// <summary>Every request, pending first and newest first within each group.</summary>
        Task<IReadOnlyList<SellerRequestDto>> GetAllAsync();

        /// <summary>Approving makes the customer a seller.</summary>
        Task<SellerRequestResult> ApproveAsync(int requestId, string reviewer);

        Task<SellerRequestResult> RejectAsync(int requestId, string reviewer);
    }
}
