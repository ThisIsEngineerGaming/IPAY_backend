using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Shop
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repository;

        public ProductService(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<Product>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Product?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<Product> CreateAsync(Product product) => _repository.AddAsync(product);

        public Task UpdateAsync(int id, Product product) => _repository.UpdateAsync(id, product);

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
