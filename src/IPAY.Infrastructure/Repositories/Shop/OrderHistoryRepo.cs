using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Persistence.Documents;
using System.Reflection;

namespace IPAY.Infrastructure.Repositories.Shop
{
    public class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
    {
        private readonly CollectionReference _collection;
        private readonly PropertyInfo IdProperty;

        public OrderStatusHistoryRepository(FirestoreDb firestoreDb)
        {
            _collection = firestoreDb.Collection("order_status_history");
            IdProperty = typeof(OrderStatusHistory).GetProperty(nameof(OrderStatusHistory.Id))!;
        }

        public async Task<OrderStatusHistory> AddAsync(OrderStatusHistory entity)
        {
            var nextId = await GetNextIdAsync();
            IdProperty.SetValue(entity, nextId);

            await _collection.Document(nextId.ToString())
                .SetAsync(OrderStatusHistoryDocument.FromEntity(entity));

            return entity;
        }

        public async Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(int orderId)
        {
            var snapshot = await _collection
                .WhereEqualTo("OrderID", orderId)
                .OrderBy("ChangedAt")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<OrderStatusHistoryDocument>().ToEntity())
                .ToList();
        }

        private async Task<int> GetNextIdAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            if (snapshot.Count == 0) return 1;

            return snapshot.Documents
                .Select(d => d.ConvertTo<OrderStatusHistoryDocument>().Id)
                .Max() + 1;
        }
    }
}
