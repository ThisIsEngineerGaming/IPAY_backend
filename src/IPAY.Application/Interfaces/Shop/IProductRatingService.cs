using IPAY.Domain.Interfaces.ForRepos.Shop;

namespace IPAY.Application.Interfaces.Shop
{
    //product rating interface(rates by customer)
    public interface IProductRatingService
    {
        /// <summary>Rates a product 1-5 as this user. Each user can rate each product once.</summary>
        Task<RateOutcome> RateAsync(int productId, string userId, int value);

        /// <summary>productId -> stars this user already gave.</summary>
        Task<IReadOnlyDictionary<int, int>> GetMineAsync(string userId);
    }
}
