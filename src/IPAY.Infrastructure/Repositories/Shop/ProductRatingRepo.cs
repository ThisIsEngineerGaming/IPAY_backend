using Google.Cloud.Firestore;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Persistence.Documents;

namespace IPAY.Infrastructure.Repositories.Shop
{
    public class ProductRatingRepo(FirestoreDb database, string productsCollection, string ratingsCollection)
        : IProductRatingRepo
    {
        public async Task<RateOutcome> RateAsync(int productId, string userId, int value)
        {
            var productRef = database.Collection(productsCollection).Document(productId.ToString());
            var ratingRef = database.Collection(ratingsCollection).Document($"{productId}_{userId}");

            // One transaction: the "already rated?" check, the new rating and the product's new average
            // either all happen or none do, so two quick clicks can't both count.
            return await database.RunTransactionAsync(async transaction =>
            {
                var productSnapshot = await transaction.GetSnapshotAsync(productRef);
                if (!productSnapshot.Exists)
                    return new RateOutcome(RateResult.ProductNotFound);

                var product = productSnapshot.ConvertTo<ProductDocument>();

                if (!string.IsNullOrEmpty(product.SellerId) && product.SellerId == userId)
                    return new RateOutcome(RateResult.OwnProduct, product.Rating, product.RatingCount);

                var ratingSnapshot = await transaction.GetSnapshotAsync(ratingRef);
                if (ratingSnapshot.Exists)
                    return new RateOutcome(RateResult.AlreadyRated, product.Rating, product.RatingCount);

                // Products created before user ratings existed carry a hand-set Rating with RatingCount 0;
                // the first real rating replaces it instead of being averaged with it.
                var count = product.RatingCount + 1;
                var average = product.RatingCount == 0
                    ? value
                    : (product.Rating * product.RatingCount + value) / count;
                average = Math.Round(average, 2);

                transaction.Set(ratingRef, new ProductRatingDocument
                {
                    ProductId = productId,
                    UserId = userId,
                    Value = value,
                    CreatedAt = DateTime.UtcNow
                });
                transaction.Update(productRef, new Dictionary<string, object>
                {
                    ["Rating"] = average,
                    ["RatingCount"] = count
                });

                return new RateOutcome(RateResult.Rated, average, count);
            });
        }

        public async Task<IReadOnlyDictionary<int, int>> GetByUserAsync(string userId)
        {
            // Single-field equality filter: no composite index needed.
            var snapshot = await database.Collection(ratingsCollection)
                .WhereEqualTo("UserId", userId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(document => document.ConvertTo<ProductRatingDocument>())
                .ToDictionary(rating => rating.ProductId, rating => rating.Value);
        }
    }
}
