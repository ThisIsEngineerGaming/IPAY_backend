

using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Persistence.Documents;
using System.Reflection;

namespace IPAY.Infrastructure.Repositories.Shop
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly CollectionReference _collection;
        private readonly PropertyInfo _idProperty;

        public ReviewRepository(FirestoreDb firestoreDb)
        {
            _collection = firestoreDb.Collection("reviews");
            _idProperty = typeof(Review).GetProperty(nameof(Review.Id))!;
        }

        public async Task<Review> AddAsync(Review entity)
        {
            var nextId = await GetNextIdAsync();
            _idProperty.SetValue(entity, nextId);

            if (entity.CreatedAt == default)
                entity.CreatedAt = DateTime.UtcNow;

            await _collection.Document(nextId.ToString())
                .SetAsync(ReviewDocument.FromEntity(entity));

            return entity;
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
            return snapshot.Exists
                ? snapshot.ConvertTo<ReviewDocument>().ToEntity()
                : null;
        }

        public async Task<IReadOnlyList<Review>> GetByProductIdAsync(int productId)
        {
            var snapshot = await _collection
                .WhereEqualTo("ProductId", productId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<ReviewDocument>().ToEntity())
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public async Task DeleteAsync(int id)
        {
            await _collection.Document(id.ToString()).DeleteAsync();
        }

        private async Task<int> GetNextIdAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            if (snapshot.Count == 0) return 1;

            return snapshot.Documents
                .Select(d => d.ConvertTo<ReviewDocument>().Id)
                .DefaultIfEmpty(0)
                .Max() + 1;
        }
    }
}
