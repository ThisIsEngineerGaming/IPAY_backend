using System.Collections.Generic;
using System.Threading.Tasks;
using IPAY.Application.DTOs.Shop;

namespace IPAY.Application.Interfaces.Shop
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(SaveCategoryDto category);
        Task UpdateCategoryAsync(int id, SaveCategoryDto category);
        Task DeleteAsync(int id);

        Task AddToCategoryAsync(int categoryId, int productId);
        Task RemoveFromCategoryAsync(int categoryId, int productId);
        Task ClearCategoryAsync(int categoryId);
    }
}
