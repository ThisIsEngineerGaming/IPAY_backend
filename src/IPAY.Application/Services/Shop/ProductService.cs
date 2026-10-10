using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;

namespace IPAY.Application.Services.Shop
{
  

    public class ProductService : IProductService
    {
        private readonly IProductRepo _repository;
        private readonly IMapper _productMapper;

        private readonly IValidator<UpdateAdminProductDto> _updateAdminValidator;
        private readonly IValidator<UpdateSellerProductDto> _updateSellerValidator;

        public ProductService(
            IProductRepo repository,
            IMapper productMapper,
            IValidator<UpdateAdminProductDto> updateAdminValidator,
            IValidator<UpdateSellerProductDto> updateSellerValidator)
        {
            _repository = repository;
            _productMapper = productMapper;
            _updateAdminValidator = updateAdminValidator;
            _updateSellerValidator = updateSellerValidator;
        }

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
        {
            var products = await _repository.GetAllProductsAsync();
            return _productMapper.Map<IReadOnlyList<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _repository.GetProductByIdAsync(id);
            return product is null ? null : _productMapper.Map<ProductDto>(product);
        }

        // ---------- ADMIN ----------

        public async Task<ProductDto> CreateAsync(CreateAdminProductDto adminProduct)
        {
            var product = _productMapper.Map<Product>(adminProduct);
            product.CreatedAt = DateTime.UtcNow;

            var created = await _repository.AddProductAsync(product);
            return _productMapper.Map<ProductDto>(created);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateAdminProductDto adminProduct)
        {
            await _updateAdminValidator.ValidateAndThrowAsync(adminProduct);

            var existing = await _repository.GetProductByIdAsync(id);
            if (existing is null) return null;

            var product = _productMapper.Map<Product>(adminProduct);
            product.Id = id;
            product.SellerId = existing.SellerId;
            product.CreatedAt = existing.CreatedAt;

            await _repository.UpdateProductAsync(id, product);

            var updated = await _repository.GetProductByIdAsync(id);
            return updated is null ? null : _productMapper.Map<ProductDto>(updated);
        }

        // ---------- SELLER ----------

        public async Task<ProductDto?> CreateAsync(CreateSellerProductDto dto, string sellerId)
        {
            var product = _productMapper.Map<Product>(dto);

            // The id is assigned by the repository. SellerId always comes from the caller's token,
            // and a seller can't rate their own listing, so rating starts at 0 whatever the body says.
            product.SellerId = sellerId;
            product.Rating = 0;
            product.CreatedAt = DateTime.UtcNow;

            var created = await _repository.AddProductAsync(product);
            return _productMapper.Map<ProductDto>(created);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateSellerProductDto sellerProduct, string? sellerId)
        {
            await _updateSellerValidator.ValidateAndThrowAsync(sellerProduct);

            var existing = await _repository.GetProductByIdAsync(id);
            if (existing is null) return null;

            if (existing.SellerId != sellerId)
                throw new UnauthorizedAccessException("Нельзя изменять чужой товар.");

            var product = _productMapper.Map<Product>(sellerProduct);
            product.Id = id;
            product.SellerId = existing.SellerId;
            product.Rating = existing.Rating; // sellers can't change their own rating
            product.CreatedAt = existing.CreatedAt;

            await _repository.UpdateProductAsync(id, product);

            var updated = await _repository.GetProductByIdAsync(id);
            return updated is null ? null : _productMapper.Map<ProductDto>(updated);
        }

        public Task DeleteAsync(int id) => _repository.DeleteProductAsync(id);

        public async Task<IReadOnlyList<ProductDto>> GetBySellerAsync(string sellerId)
        {
            var products = await _repository.GetBySellerAsync(sellerId);
            return _productMapper.Map<IReadOnlyList<ProductDto>>(products);
        }

        public async Task<IReadOnlyList<ProductDto>> GetLimitAsync(int limit, string? lastDocId)
        {
            var products = await _repository.GetLimitedProduct(limit, lastDocId);
            return _productMapper.Map<List<ProductDto>>(products);
        }

        public async Task<IReadOnlyList<ProductDto?>> GetFilteredAsync(
            int limit, string? lastDocId, int? categoryId,
            double? minPrice, double? maxPrice, string? brand, string? search)
        {
            var result = await _repository.GetFilteredAsync(limit, lastDocId, categoryId, minPrice, maxPrice, brand, search);
            return _productMapper.Map<List<ProductDto>>(result);
        }

        public async Task<IReadOnlyList<ProductDto>> GetSortedAsync(
            int limit, string? lastDocId, string sortBy = "price", string sortDir = "asc")
        {
            var products = await _repository.GetSortedAsync(limit, lastDocId, sortBy, sortDir);
            return _productMapper.Map<List<ProductDto>>(products);
        }
    }
}
