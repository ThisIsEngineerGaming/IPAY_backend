using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPAY.Application.Services.Shop
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

        public async Task<ProductDto> CreateAsync(CreateAdminProductDto adminProduct)
        {
            // 1. DTO → Entity
            var product = _productMapper.Map<Product>(adminProduct);

            product.CreatedAt = DateTime.UtcNow;

            // 2. Сохраняем Entity
            var created = await _repository.AddProductAsync(product);

            // 3. Entity → DTO
            return _productMapper.Map<ProductDto>(created);
        }



        public async Task<ProductDto?> UpdateAsync(int id, UpdateAdminProductDto adminProduct)
        {

            var product = _productMapper.Map<Product>(adminProduct);
            await _repository.UpdateProductAsync(id, product);

            var updated = await _repository.GetProductByIdAsync(id);
            return updated is null ? null : _productMapper.Map<ProductDto>(updated);
        }


        public async Task<ProductDto?> CreateAsync(CreateSellerProductDto dto, string sellerId)
        {
            var product = _productMapper.Map<Product>(dto);

            var lastId = await _repository.GetAllProductsAsync();

            product.Id = lastId.Count > 0 ? lastId.Max(p => p.Id) + 1 : 1;
            product.SellerId = sellerId;

            await _repository.AddProductAsync(product);

            return _productMapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateSellerProductDto sellerProduct)
        {
            var existing = await _repository.GetProductByIdAsync(id);
            if (existing is null) return null;

            var product = _productMapper.Map<Product>(sellerProduct);
            product.Id = id;
            product.SellerId = existing.SellerId; // зберігаємо, щоб не затерти при update

            await _repository.UpdateProductAsync(id, product);

            var updated = await _repository.GetProductByIdAsync(id);
            return updated is null ? null : _productMapper.Map<ProductDto>(updated);
        }

        public Task DeleteAsync(int id) => _repository.DeleteProductAsync(id);

        public async Task<IReadOnlyList<ProductDto>> GetLimitAsync(int limit, string? lastDocId) {

            var products = await _repository.GetLimitedProduct(limit, lastDocId ?? null);
            return _productMapper.Map<List<ProductDto>>(products);

        }

        public async Task<IReadOnlyList<ProductDto?>> GetFilteredAsync(int limit, string? lastDocId, int? categoryId, double? minPrice, double? maxPrice, string? brand, string? search) { 
            var result=await _repository.GetFilteredAsync(limit, lastDocId, categoryId, minPrice, maxPrice, brand,search);
            return _productMapper.Map<List<ProductDto>>(result);
        }

        public async Task<IReadOnlyList<ProductDto>> GetSortedAsync(
           int limit,
           string? lastDocId,
           string sortBy = "price",
           string sortDir = "asc")
        {
            var products = await _repository.GetSortedAsync(limit, lastDocId, sortBy, sortDir);
           return _productMapper.Map<List<ProductDto>>(products);


        }


    }
}
