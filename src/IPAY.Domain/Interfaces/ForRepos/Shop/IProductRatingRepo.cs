namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    public enum RateResult
    {
        Rated,
        ProductNotFound,
        AlreadyRated,
        OwnProduct
    }

    /// <summary>What happened when someone tried to rate a product. Rating/RatingCount are the product's values afterwards.</summary>
    public record RateOutcome(RateResult Result, double Rating = 0, int RatingCount = 0);

    public interface IProductRatingRepo
    {
        /// <summary>
        /// Records one rating (1-5) and updates the product's average. A user can rate a product only once,
        /// and never their own product; both are checked atomically together with the write.
        /// </summary>
        Task<RateOutcome> RateAsync(int productId, string userId, int value);

        /// <summary>productId -> stars this user gave.</summary>
        Task<IReadOnlyDictionary<int, int>> GetByUserAsync(string userId);
    }
}
