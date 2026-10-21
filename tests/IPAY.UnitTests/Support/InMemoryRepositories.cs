using System.Text.Json;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Entities.Auth;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Domain.Interfaces.ForRepos.Auth;
using IPAY.Domain.Interfaces.ForRepos.Shop;

namespace IPAY.UnitTests.Support
{
    /// <summary>
    /// Stand-in for the Firestore repositories used by the integration tests.
    /// Like Firestore, it stores copies: mutating an object you got back changes nothing
    /// until you call UpdateAsync, so "forgot to save" bugs show up here too.
    /// </summary>
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly Dictionary<int, string> _rows = new();
        private readonly Func<T, int> _getId;
        private readonly Action<T, int> _setId;
        private int _nextId = 1;

        public InMemoryRepository(Func<T, int> getId, Action<T, int> setId)
        {
            _getId = getId;
            _setId = setId;
        }

        private static string Clone(T value) => JsonSerializer.Serialize(value);
        private static T Restore(string json) => JsonSerializer.Deserialize<T>(json)!;

        public Task<IReadOnlyList<T>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<T>>(_rows.Values.Select(Restore).ToList());

        public Task<T?> GetByIdAsync(int id) =>
            Task.FromResult(_rows.TryGetValue(id, out var json) ? Restore(json) : null);

        public Task<T> AddAsync(T entity)
        {
            var id = _nextId++;
            _setId(entity, id);
            _rows[id] = Clone(entity);
            return Task.FromResult(Restore(_rows[id]));
        }

        public Task UpdateAsync(int id, T entity)
        {
            _setId(entity, id);
            _rows[id] = Clone(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            _rows.Remove(id);
            return Task.CompletedTask;
        }
    }

    public class InMemoryProductRepo : InMemoryRepository<Product>, IProductRepo
    {
        public InMemoryProductRepo() : base(p => p.Id, (p, id) => p.Id = id) { }

        public Task<IReadOnlyList<Product>> GetAllProductsAsync() => GetAllAsync();
        public Task<Product?> GetProductByIdAsync(int id) => GetByIdAsync(id);
        public Task<Product> AddProductAsync(Product entity) => AddAsync(entity);
        public Task UpdateProductAsync(int id, Product entity) => UpdateAsync(id, entity);
        public Task DeleteProductAsync(int id) => DeleteAsync(id);

        public async Task<IReadOnlyList<Product>> GetBySellerAsync(string sellerId) =>
            (await GetAllAsync()).Where(p => p.SellerId == sellerId).OrderByDescending(p => p.CreatedAt).ToList();

        public async Task<IReadOnlyList<Product>> GetLimitedProduct(int limit, string? lastDocId) =>
            (await GetAllAsync()).Take(limit).ToList();

        public async Task<IReadOnlyList<Product>> GetFilteredAsync(int limit, string? lastDocId, int? categoryId,
            double? minPrice, double? maxPrice, string? brand, string? search) =>
            (await GetAllAsync()).Where(p =>
                    (categoryId is null || p.CategoryId == categoryId) &&
                    (minPrice is null || p.Price >= minPrice) &&
                    (maxPrice is null || p.Price <= maxPrice) &&
                    (brand is null || p.Manufacturer == brand) &&
                    (search is null || p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .Take(limit).ToList();

        public async Task<IReadOnlyList<Product>> GetSortedAsync(int limit, string? lastDocId, string sortBy, string sortDir)
        {
            var all = await GetAllAsync();
            var sorted = sortDir == "desc" ? all.OrderByDescending(p => p.Price) : all.OrderBy(p => p.Price);
            return sorted.Take(limit).ToList();
        }
    }

    public class InMemoryOrderRepository : InMemoryRepository<Order>, IOrderRepository
    {
        public InMemoryOrderRepository() : base(o => o.Id, (o, id) => o.Id = id) { }

        public async Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId) =>
            (await GetAllAsync()).Where(o => o.UserId == userId).ToList();

        public Task<Order?> GetByIdWithItemsAsync(int id) => GetByIdAsync(id);
    }

    public class InMemoryOrderItemRepository : InMemoryRepository<OrderItem>, IOrderItemRepository
    {
        public InMemoryOrderItemRepository() : base(i => i.Id, (i, id) => i.Id = id) { }

        public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(int orderId) =>
            (await GetAllAsync()).Where(i => i.OrderId == orderId).ToList();
    }

    public class InMemoryOrderStatusHistoryRepository : IOrderStatusHistoryRepository
    {
        private readonly List<OrderStatusHistory> _rows = new();
        private int _nextId = 1;

        public Task<OrderStatusHistory> AddAsync(OrderStatusHistory entity)
        {
            entity.Id = _nextId++;
            _rows.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(int orderId) =>
            Task.FromResult<IReadOnlyList<OrderStatusHistory>>(_rows.Where(h => h.OrderId == orderId).ToList());
    }

    public class InMemoryRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly Dictionary<string, RefreshToken> _rows = new();

        public Task AddAsync(RefreshToken token)
        {
            _rows[token.Id] = token;
            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetByIdAsync(string id) =>
            Task.FromResult<RefreshToken?>(_rows.TryGetValue(id, out var token) ? token : null);

        public Task UpdateAsync(RefreshToken token)
        {
            _rows[token.Id] = token;
            return Task.CompletedTask;
        }

        public Task RevokeAllForUserAsync(int userId)
        {
            foreach (var token in _rows.Values.Where(t => t.UserId == userId))
                token.IsRevoked = true;
            return Task.CompletedTask;
        }
    }

    public class InMemoryAddressRepository : InMemoryRepository<Address>, IAddressRepository
    {
        public InMemoryAddressRepository() : base(a => a.Id, (a, id) => a.Id = id) { }

        public async Task<IReadOnlyList<Address>> GetByUserIdAsync(int userId) =>
            (await GetAllAsync()).Where(a => a.UserId == userId).ToList();

        public async Task<Address?> GetByUserIdAndIdAsync(int userId, int addressId) =>
            (await GetAllAsync()).FirstOrDefault(a => a.UserId == userId && a.Id == addressId);
    }
}
