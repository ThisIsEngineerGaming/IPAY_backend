using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos;

namespace IPAY.Application.Services.Shop
{
    //category service
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _repository;
        private readonly IMapper _mapper;//dto->object object->dto

        public CategoryService(IRepository<Category> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        //get all
        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(all);
        }
        //get by id
        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return category is null ? null : _mapper.Map<CategoryDto>(category);
        }
        //create
        public async Task<CategoryDto> CreateAsync(SaveCategoryDto category)
        {
            var created = await _repository.AddAsync(_mapper.Map<Category>(category));
            return _mapper.Map<CategoryDto>(created);
        }
        //update
        public Task UpdateCategoryAsync(int id, SaveCategoryDto category) =>
            _repository.UpdateAsync(id, _mapper.Map<Category>(category));
        //delete
        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
        //add to category
        public async Task AddToCategoryAsync(int categoryId, int productId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category is null) return;
            if (!category.ProductIds.Contains(productId)) category.ProductIds.Add(productId);
            category.Quantity = category.ProductIds.Count;
            await _repository.UpdateAsync(categoryId, category);
        }
        //remove from category
        public async Task RemoveFromCategoryAsync(int categoryId, int productId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category is null) return;
            category.ProductIds.Remove(productId);
            category.Quantity = category.ProductIds.Count;
            await _repository.UpdateAsync(categoryId, category);
        }
        //clear all items
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
