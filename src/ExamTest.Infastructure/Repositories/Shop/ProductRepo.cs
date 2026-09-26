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
        public async Task<IReadOnlyList<Product>> GetFilteredAsync(
            int limit,
            string? lastDocId,
            int? categoryId,
            double? minPrice,
            double? maxPrice,
            string? brand,
            string? search)
        {
            Query query = _collection;

            // --- equality-фильтры ---
            if (categoryId.HasValue)
                query = query.WhereEqualTo("CategoryId", categoryId.Value);

            if (!string.IsNullOrWhiteSpace(brand))
                query = query.WhereEqualTo("Manufacturer", brand.Trim());

            var hasSearch = !string.IsNullOrWhiteSpace(search);
            var hasPrice = minPrice.HasValue || maxPrice.HasValue;

            // Firestore: range только по ОДНОМУ полю
            // если есть search — фильтруем по NameLower (цену в этом запросе не мешаем)
            if (hasSearch)
            {
                var term = search!.Trim().ToLowerInvariant();
                query = query
                    .WhereGreaterThanOrEqualTo("NameLower", term)
                    .WhereLessThanOrEqualTo("NameLower", term + "\uf8ff");

                query = query.OrderBy("NameLower");
            }
            else
            {
                if (minPrice.HasValue)
                    query = query.WhereGreaterThanOrEqualTo("Price", minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.WhereLessThanOrEqualTo("Price", maxPrice.Value);

                if (hasPrice)
                    query = query.OrderBy("Price");
                else
                    query = query.OrderBy(FieldPath.DocumentId);
            }

            query = query.Limit(limit);

            // --- cursor ---
            if (!string.IsNullOrEmpty(lastDocId))
            {
                var lastSnap = await _collection.Document(lastDocId).GetSnapshotAsync();
                if (lastSnap.Exists)
                {
                    if (hasSearch)
                    {
                        // курсор по тому же полю, что и OrderBy
                        var nameLower = lastSnap.GetValue<string>("NameLower");
                        query = query.StartAfter(nameLower);
                    }
                    else if (hasPrice)
                    {
                        var price = lastSnap.GetValue<double>("Price");
                        query = query.StartAfter(price);
                    }
                    else
                    {
                        query = query.StartAfter(lastSnap);
                    }
                }
            }

            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<ProductDocument>().ToEntity())
                .ToList();
        }
        public async Task<IReadOnlyList<Product>> GetSortedAsync(
    int limit,
    string? lastDocId,
    string sortBy,
    string sortDir)
        {
            Query query = _collection;

            var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

            // какое поле сортируем
            var field = sortBy?.Trim().ToLowerInvariant() switch
            {
                "rating" => "Rating",
                "date" => "CreatedAt",
                _ => "Price"          // по умолчанию цена
            };

            query = descending
                ? query.OrderByDescending(field)
                : query.OrderBy(field);

            query = query.Limit(limit);

            // cursor
            if (!string.IsNullOrEmpty(lastDocId))
            {
                var lastSnap = await _collection.Document(lastDocId).GetSnapshotAsync();
                if (lastSnap.Exists)
                {
                    // курсор по тому же полю, что и OrderBy
                    if (field == "CreatedAt")
                    {
                        var value = lastSnap.GetValue<Timestamp>("CreatedAt");
                        query = descending
                            ? query.StartAfter(value)  // для desc тоже StartAfter от snapshot-значения
                            : query.StartAfter(value);
                    }
                    else
                    {
                        var value = lastSnap.GetValue<double>(field);
                        query = query.StartAfter(value);
                    }
                }
            }

            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<ProductDocument>().ToEntity())
                .ToList();
        }

    }
}
