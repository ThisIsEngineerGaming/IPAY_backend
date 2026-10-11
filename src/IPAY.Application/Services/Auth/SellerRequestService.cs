using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos;

namespace IPAY.Application.Services.Auth
{
    public class SellerRequestService(IRepository<SellerRequest> requests, IUser<Customer> customers)
        : ISellerRequestService
    {
        private const int MaxMessageLength = 500;

        public async Task<SellerRequestResult> CreateAsync(int userId, string? message)
        {
            message = message?.Trim() ?? string.Empty;
            if (message.Length > MaxMessageLength)
                return new SellerRequestResult(null, $"The message can be at most {MaxMessageLength} characters.");

            var user = await customers.GetByIdAsync(userId);
            if (user is null)
                return new SellerRequestResult(null, "User not found.", NotFound: true);

            if (user.IsBanned)
                return new SellerRequestResult(null, "Banned accounts can't request seller status.");
            if (user.Role == UserRole.Seller)
                return new SellerRequestResult(null, "You are already a seller.");
            if (user.Role != UserRole.Customer)
                return new SellerRequestResult(null, "Only customers can request to become a seller.");

            var all = await requests.GetAllAsync();
            if (all.Any(r => r.UserId == userId && r.Status == SellerRequestStatus.Pending))
                return new SellerRequestResult(null, "You already have a request waiting for review.");

            var created = await requests.AddAsync(new SellerRequest
            {
                UserId = userId,
                UserName = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Message = message,
                Status = SellerRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

            return new SellerRequestResult(ToDto(created));
        }

        public async Task<SellerRequestDto?> GetLatestForUserAsync(int userId)
        {
            var all = await requests.GetAllAsync();
            var latest = all
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            return latest is null ? null : ToDto(latest);
        }

        public async Task<IReadOnlyList<SellerRequestDto>> GetAllAsync()
        {
            var all = await requests.GetAllAsync();
            return all
                .OrderBy(r => r.Status == SellerRequestStatus.Pending ? 0 : 1)
                .ThenByDescending(r => r.CreatedAt)
                .Select(ToDto)
                .ToList();
        }

        public Task<SellerRequestResult> ApproveAsync(int requestId, string reviewer) =>
            ReviewAsync(requestId, reviewer, approve: true);

        public Task<SellerRequestResult> RejectAsync(int requestId, string reviewer) =>
            ReviewAsync(requestId, reviewer, approve: false);

        private async Task<SellerRequestResult> ReviewAsync(int requestId, string reviewer, bool approve)
        {
            var request = await requests.GetByIdAsync(requestId);
            if (request is null)
                return new SellerRequestResult(null, "Request not found.", NotFound: true);

            if (request.Status != SellerRequestStatus.Pending)
                return new SellerRequestResult(null, "This request has already been reviewed.");

            if (approve)
            {
                var user = await customers.GetByIdAsync(request.UserId);
                if (user is null)
                    return new SellerRequestResult(null, "This user no longer exists.");
                if (user.IsBanned)
                    return new SellerRequestResult(null, "This account is banned. Unban it before approving.");
                if (user.Role is UserRole.Admin or UserRole.Moderator)
                    return new SellerRequestResult(null, "This user is already staff and can't become a seller.");

                // Already a seller (e.g. made one directly on the Users tab): just close the request.
                if (user.Role != UserRole.Seller)
                {
                    user.Role = UserRole.Seller;
                    await customers.UpdateAsync(request.UserId, user);
                }
            }

            request.Status = approve ? SellerRequestStatus.Approved : SellerRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedBy = reviewer;
            await requests.UpdateAsync(requestId, request);

            return new SellerRequestResult(ToDto(request));
        }

        private static SellerRequestDto ToDto(SellerRequest r) => new()
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.UserName,
            Email = r.Email,
            Message = r.Message,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt
        };
    }
}
