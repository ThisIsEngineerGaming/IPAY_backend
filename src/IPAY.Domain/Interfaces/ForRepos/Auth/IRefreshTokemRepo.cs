using IPAY.Domain.Entities.Auth;

namespace IPAY.Domain.Interfaces.ForRepos.Auth
{
    //interface for refreshing JWT token(because we made a limited time for KWT)
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByIdAsync(string id);
        Task UpdateAsync(RefreshToken token);
        Task RevokeAllForUserAsync(int userId);
    }
}
