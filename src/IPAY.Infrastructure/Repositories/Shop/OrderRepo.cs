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
    public class OrderRepository : IOrderRepository
    {
        private readonly CollectionReference _collection;
        private readonly PropertyInfo IdProperty;

        public OrderRepository(FirestoreDb firestoreDb)
        {
            _collection = firestoreDb.Collection("orders");
            IdProperty = typeof(Order).GetProperty(nameof(Order.Id))!;
        }

        public async Task<IReadOnlyList<Order>> GetAllAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(doc => doc.ConvertTo<OrderDocument>().ToEntity())
                .ToList();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
            return snapshot.Exists
                ? snapshot.ConvertTo<OrderDocument>().ToEntity()
                : null;
        }

        public async Task<Order?> GetByIdWithItemsAsync(int id)
        {
            // В Firestore всё уже лежит вместе, так что просто GetById
            return await GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId)
        {
            var snapshot = await _collection
                .WhereEqualTo("UserId", userId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<OrderDocument>().ToEntity())
                .ToList();
        }

        public async Task<Order> AddAsync(Order entity)
        {
            var nextId = await GetNextIdAsync();
            IdProperty.SetValue(entity, nextId);

            // Проставляем OrderId айтемам
            foreach (var item in entity.Items)
            {
                item.OrderId = nextId;
            }

            await _collection.Document(nextId.ToString())
                .SetAsync(OrderDocument.FromEntity(entity));

            return entity;
        }

        public async Task UpdateAsync(int id, Order entity)
        {
            IdProperty.SetValue(entity, id);
            await _collection.Document(id.ToString())
                .SetAsync(OrderDocument.FromEntity(entity));
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
