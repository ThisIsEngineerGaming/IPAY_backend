using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;
using ExamTest.Domain.Interfaces.ForRepos.Shop;

namespace ExamTest.Application.Services.Shop
{
    public class ProductService : IProductService
    {
        private readonly IProductRepo _repository;

        private readonly IMapper _productMapper;

        public ProductService(IProductRepo repository,IMapper productMapper)
        {
            _repository = repository;
            _productMapper = productMapper;
        }

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync() {
            var products = await _repository.GetAllProductsAsync();
            return _productMapper.Map<IReadOnlyList<ProductDto>>(products); 
        }

        public async Task<ProductDto?> GetByIdAsync(int id) { 
            var products = await _repository.GetProductByIdAsync(id);
            return _productMapper.Map<ProductDto>(products);
        }

        public Task<Product> CreateAsync(Product product) {
            return _repository.AddProductAsync(product);
        }

        public Task UpdateAsync(int id, Product product) {
           
            return _repository.UpdateProductAsync(id, product);
        }

        public Task DeleteAsync(int id) => _repository.DeleteProductAsync(id);

        public async Task<IReadOnlyList<ProductDto>> GetLimitAsync(int limit, string? lastDocId) {

            var products = await _repository.GetLimitedProduct(limit, lastDocId ?? null);
            return _productMapper.Map<IReadOnlyList<ProductDto>>(products);

        }
    }
}
