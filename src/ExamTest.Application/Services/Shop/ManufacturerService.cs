using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.Domain.Entities.Shop;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Shop
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly IRepository<Manufacturer> _repository;
        private readonly IMapper _mapper;

        public ManufacturerService(IRepository<Manufacturer> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ManufacturerDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<ManufacturerDto>>(all);
        }

        public async Task<ManufacturerDto?> GetByIdAsync(int id)
        {
            var manufacturer = await _repository.GetByIdAsync(id);
            return manufacturer is null ? null : _mapper.Map<ManufacturerDto>(manufacturer);
        }

        public async Task<ManufacturerDto> CreateAsync(SaveManufacturerDto manufacturer)
        {
            var created = await _repository.AddAsync(_mapper.Map<Manufacturer>(manufacturer));
            return _mapper.Map<ManufacturerDto>(created);
        }

        public Task UpdateAsync(int id, SaveManufacturerDto manufacturer) =>
            _repository.UpdateAsync(id, _mapper.Map<Manufacturer>(manufacturer));

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
