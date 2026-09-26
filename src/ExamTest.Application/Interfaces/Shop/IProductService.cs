using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Domain.Entities.Shop;

namespace ExamTest.Application.Interfaces.Shop
{
    public interface IProductService
    {
        Task<IReadOnlyList<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateAdminProductDto adminProduct);
        Task<ProductDto?> UpdateAsync(int id, UpdateAdminProductDto adminProduct);
        Task DeleteAsync(int id);

        Task<IReadOnlyList<ProductDto>> GetLimitAsync(int limit, string? lastDocid);

        Task<IReadOnlyList<ProductDto?>> GetFilteredAsync(int limit, string? lastDocId, int? categoryId, double? minPrice, double? maxPrice, string? brand, string? search);

        Task<IReadOnlyList<ProductDto>> GetSortedAsync(
    int limit,
    string? lastDocId,
    string sortBy,    // price | rating | date
    string sortDir);  // asc | desc

        // "createCard(container:div)" on the diagram is a DOM/UI concern for the
        // frontend to implement - there's nothing meaningful for a backend to do there,
        // so it's intentionally left out here.
    }
}
