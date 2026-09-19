using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos.Shop;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ExamTest.Infastructure.Repositories.Shop
{
    public class ProductRepo(FirestoreDb database, string collectionName) : IProductRepo
    {
        private readonly CollectionReference _collection = database.Collection(collectionName);
        private static readonly PropertyInfo IdProperty = typeof(Product).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(Product).Name} must have a public 'Id' property.");

        public async Task<IReadOnlyList<Product>> GetAllProductsAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents.Select(document => document.ConvertTo<Product>()).ToList();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Product>() : null;
        }

        public async Task<Product> AddProductAsync(Product entity)
        {
            var nextId = await GetNextIdAsync();
            IdProperty.SetValue(entity, nextId);
            await _collection.Document(nextId.ToString()).SetAsync(entity);
            return entity;
        }

        public async Task UpdateProductAsync(int id, Product entity)
        {
            IdProperty.SetValue(entity, id);
            await _collection.Document(id.ToString()).SetAsync(entity);
        }

        public Task DeleteProductAsync(int id) => _collection.Document(id.ToString()).DeleteAsync();

        private async Task<int> GetNextIdAsync()
        {
            var all = await GetAllProductsAsync();
            return all.Count == 0 ? 1 : all.Max(entity => (int)IdProperty.GetValue(entity)!) + 1;
        }

        public async Task<IReadOnlyList<Product>> GetLimitedProduct(int limit, string? lastDocId)
        {
            Query query = _collection
                .OrderBy(FieldPath.DocumentId)  // или OrderBy("Id") — главное, чтобы был порядок
                .Limit(limit);

            // Если передан курсор — начинаем после него
            if (!string.IsNullOrEmpty(lastDocId))
            {
                var lastDocSnapshot = await _collection
                    .Document(lastDocId)
                    .GetSnapshotAsync();

                if (lastDocSnapshot.Exists)
                {
                    query = query.StartAfter(lastDocSnapshot);
                }
            }

            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Product>())
                .ToList();
        }
    }
}
