using IPAY.Domain.Entities.Auth;

namespace IPAY.Domain.Interfaces.ForRepos.Auth
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByIdAsync(string id);
        Task UpdateAsync(RefreshToken token);
        Task RevokeAllForUserAsync(int userId);
    }
}
