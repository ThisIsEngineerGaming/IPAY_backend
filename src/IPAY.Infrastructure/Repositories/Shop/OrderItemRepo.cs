using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IPAY.Infrastructure.Repositories.Shop
{
    public class OrderItemRepository : IOrderItemRepository
    {
<<<<<<< Updated upstream
        private const string CollectionName = "orderItems";

        private readonly FirestoreDb _database;
=======
        
>>>>>>> Stashed changes
        private readonly CollectionReference _collection;
        private readonly PropertyInfo IdProperty;

        public OrderItemRepository(FirestoreDb firestoreDb)
        {
            _database = firestoreDb;
            _collection = firestoreDb.Collection(CollectionName);
            IdProperty = typeof(OrderItem).GetProperty(nameof(OrderItem.Id))!;
        }
        //get all
        public async Task<IReadOnlyList<OrderItem>> GetAllAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(doc => doc.ConvertTo<OrderItemDocument>().ToEntity())
                .ToList();
        }
        //get by id
        public async Task<OrderItem?> GetByIdAsync(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
            return snapshot.Exists
                ? snapshot.ConvertTo<OrderItemDocument>().ToEntity()
                : null;
        }
        //get by order id
        public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            var snapshot = await _collection
                .WhereEqualTo("OrderId", orderId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<OrderItemDocument>().ToEntity())
                .ToList();
        }
        //add
        public async Task<OrderItem> AddAsync(OrderItem entity)
        {
            var nextId = await GetNextIdAsync();
            IdProperty.SetValue(entity, nextId);

            await _collection.Document(nextId.ToString())
                .SetAsync(OrderItemDocument.FromEntity(entity));

            return entity;
        }
        //update
        public async Task UpdateAsync(int id, OrderItem entity)
        {
            IdProperty.SetValue(entity, id);
            await _collection.Document(id.ToString())
                .SetAsync(OrderItemDocument.FromEntity(entity));
        }
        //delete
        public Task DeleteAsync(int id) =>
            _collection.Document(id.ToString()).DeleteAsync();
<<<<<<< Updated upstream

        // One counter document instead of reading every order item for each insert.
        private Task<int> GetNextIdAsync() =>
            FirestoreIdGenerator.NextIdAsync(_database, CollectionName, GetMaxExistingIdAsync);

        private async Task<int> GetMaxExistingIdAsync()
=======
        //get next id
        private async Task<int> GetNextIdAsync()
>>>>>>> Stashed changes
        {
            var all = await GetAllAsync();
            return all.Count == 0 ? 0 : all.Max(x => x.Id);
        }
    }
}
