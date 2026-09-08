using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Domain.Entities.Shop;

namespace ExamTest.Application.Interfaces.Shop
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task UpdateCategoryAsync(int id, Category category);
        Task DeleteAsync(int id);

        Task AddToCategoryAsync(int categoryId, int productId);
        Task RemoveFromCategoryAsync(int categoryId, int productId);
        Task ClearCategoryAsync(int categoryId);
    }
}
