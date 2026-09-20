using ExamTest.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;



namespace ExamTest.Domain.Interfaces.ForRepos.Shop
{
    public  interface IProductRepo
    {
        Task<IReadOnlyList<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(Product entity);
        Task UpdateProductAsync(int id, Product entity);
        Task DeleteProductAsync(int id);

        Task<IReadOnlyList<Product>> GetLimitedProduct(int limit, string? lastDocId);
    }
}
