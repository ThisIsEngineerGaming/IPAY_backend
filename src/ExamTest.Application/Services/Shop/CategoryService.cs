using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Shop
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _repository;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(all);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return category is null ? null : _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(SaveCategoryDto category)
        {
            var created = await _repository.AddAsync(_mapper.Map<Category>(category));
            return _mapper.Map<CategoryDto>(created);
        }

        public Task UpdateCategoryAsync(int id, SaveCategoryDto category) =>
            _repository.UpdateAsync(id, _mapper.Map<Category>(category));

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
