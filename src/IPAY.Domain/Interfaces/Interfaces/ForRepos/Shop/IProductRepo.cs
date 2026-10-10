using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;



namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    public  interface IProductRepo
    {
        Task<IReadOnlyList<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(Product entity);
        Task UpdateProductAsync(int id, Product entity);
        Task DeleteProductAsync(int id);

        /// <summary>Every product whose SellerId matches, newest first.</summary>
        Task<IReadOnlyList<Product>> GetBySellerAsync(string sellerId);

        Task<IReadOnlyList<Product>> GetLimitedProduct(int limit, string? lastDocId);

        Task<IReadOnlyList<Product>> GetFilteredAsync(int limit,string? lastDocId,int? categoryId,double? minPrice,double? maxPrice,string? brand, string? search);

        Task<IReadOnlyList<Product>> GetSortedAsync(
    int limit,
    string? lastDocId,
    string sortBy,    // price | rating | date
    string sortDir);  // asc | desc
    }
}
