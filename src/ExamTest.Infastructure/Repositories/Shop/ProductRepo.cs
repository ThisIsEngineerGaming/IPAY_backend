using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos.Shop;
using ExamTest.Infastructure.Firebase.Documents;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Repositories.Shop
{
    public class ProductRepo(FirestoreDb database, string collectionName) : IProductRepo
    {
        private readonly CollectionReference _collection = database.Collection(collectionName);

        public async Task<IReadOnlyList<Product>> GetAllProductsAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(document => document.ConvertTo<ProductDocument>().ToEntity())
                .ToList();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<ProductDocument>().ToEntity() : null;
        }

        public async Task<Product> AddProductAsync(Product entity)
        {
            entity.Id = await GetNextIdAsync();
            await _collection.Document(entity.Id.ToString()).SetAsync(ProductDocument.FromEntity(entity));
            return entity;
        }

        public async Task UpdateProductAsync(int id, Product entity)
        {
            entity.Id = id;
            await _collection.Document(id.ToString()).SetAsync(ProductDocument.FromEntity(entity));
        }

        public Task DeleteProductAsync(int id) => _collection.Document(id.ToString()).DeleteAsync();

        private async Task<int> GetNextIdAsync()
        {
            var all = await GetAllProductsAsync();
            return all.Count == 0 ? 1 : all.Max(product => product.Id) + 1;
        }

        public async Task<IReadOnlyList<Product>> GetLimitedProduct(int limit, string? lastDocId)
        {
            Query query = _collection
                .OrderBy(FieldPath.DocumentId)  // stable order is required for cursor paging
                .Limit(limit);

            // A cursor was passed - continue after that document.
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
                .Select(doc => doc.ConvertTo<ProductDocument>().ToEntity())
                .ToList();
        }
    }
}
