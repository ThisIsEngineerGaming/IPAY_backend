using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;

namespace IPAY.Application.Services.Shop
{
    public class ProductRatingService(IProductRatingRepo repository) : IProductRatingService
    {
        public Task<RateOutcome> RateAsync(int productId, string userId, int value)
        {
            if (value is < 1 or > 5)
                throw new ArgumentOutOfRangeException(nameof(value), "A rating must be between 1 and 5.");

            return repository.RateAsync(productId, userId, value);
        }

        public Task<IReadOnlyDictionary<int, int>> GetMineAsync(string userId) =>
            repository.GetByUserAsync(userId);
    }
}
