using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Shop
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _repository;

        public CategoryService(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<Category>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Category?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<Category> CreateAsync(Category category) => _repository.AddAsync(category);

        public Task UpdateCategoryAsync(int id, Category category) => _repository.UpdateAsync(id, category);

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

        public async Task AddToCategoryAsync(int categoryId, int productId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category is null) return;
            if (!category.ProductIds.Contains(productId)) category.ProductIds.Add(productId);
            category.Quantity = category.ProductIds.Count;
            await _repository.UpdateAsync(categoryId, category);
        }

        public async Task RemoveFromCategoryAsync(int categoryId, int productId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category is null) return;
            category.ProductIds.Remove(productId);
            category.Quantity = category.ProductIds.Count;
            await _repository.UpdateAsync(categoryId, category);
        }

        public async Task ClearCategoryAsync(int categoryId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category is null) return;
            category.ProductIds.Clear();
            category.Quantity = 0;
            await _repository.UpdateAsync(categoryId, category);
        }
    }
}
