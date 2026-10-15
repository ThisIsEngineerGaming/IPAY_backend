using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Persistence.Documents;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IPAY.Infrastructure.Repositories.Shop
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly CollectionReference _collection;
        private readonly PropertyInfo IdProperty;

        public OrderItemRepository(FirestoreDb firestoreDb)
        {
            _collection = firestoreDb.Collection("orderItems");
            IdProperty = typeof(OrderItem).GetProperty(nameof(OrderItem.Id))!;
        }

        public async Task<IReadOnlyList<OrderItem>> GetAllAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(doc => doc.ConvertTo<OrderItemDocument>().ToEntity())
                .ToList();
        }

        public async Task<OrderItem?> GetByIdAsync(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
            return snapshot.Exists
                ? snapshot.ConvertTo<OrderItemDocument>().ToEntity()
                : null;
        }

        public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            var snapshot = await _collection
                .WhereEqualTo("OrderId", orderId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<OrderItemDocument>().ToEntity())
                .ToList();
        }

        public async Task<OrderItem> AddAsync(OrderItem entity)
        {
            var nextId = await GetNextIdAsync();
            IdProperty.SetValue(entity, nextId);

            await _collection.Document(nextId.ToString())
                .SetAsync(OrderItemDocument.FromEntity(entity));

            return entity;
        }

        public async Task UpdateAsync(int id, OrderItem entity)
        {
            IdProperty.SetValue(entity, id);
            await _collection.Document(id.ToString())
                .SetAsync(OrderItemDocument.FromEntity(entity));
        }

        public Task DeleteAsync(int id) =>
            _collection.Document(id.ToString()).DeleteAsync();

        private async Task<int> GetNextIdAsync()
        {
            var all = await GetAllAsync();
            return all.Count == 0 ? 1 : all.Max(x => x.Id) + 1;
        }
    }
}
