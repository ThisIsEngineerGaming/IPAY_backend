using IPAY.Application.DTOs.Auth;
using IPAY.Domain.Enums;

namespace IPAY.Application.Interfaces.Auth
{
    //interface, which shows you how to refresh JWT token
    public interface IRefreshTokenService
    {
        Task<(string rawRefresh, DateTime accessExpires)> IssuePairAsync(
            int userId, string email, UserRole role, string? ip = null);

        Task<AuthResponse?> RefreshAsync(string rawRefreshToken, string? ip = null);

        Task RevokeAsync(string rawRefreshToken);
        Task RevokeAllForUserAsync(int userId);

        // IRefreshTokenService.cs
        Task<AuthResponse> CreateAuthResponseAsync(
            int userId,
            string email,
            string? name,
            UserRole role,
            string? ip = null);
    }
}
