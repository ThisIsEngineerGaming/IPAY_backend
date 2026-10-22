using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos;

namespace IPAY.Application.Services.Shop
{
    //manufacturer service
    public class ManufacturerService : IManufacturerService
    {
        private readonly IRepository<Manufacturer> _repository;
        private readonly IMapper _mapper;

        public ManufacturerService(IRepository<Manufacturer> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        //get all
        public async Task<IReadOnlyList<ManufacturerDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<ManufacturerDto>>(all);
        }
        //get by id
        public async Task<ManufacturerDto?> GetByIdAsync(int id)
        {
            var manufacturer = await _repository.GetByIdAsync(id);
            return manufacturer is null ? null : _mapper.Map<ManufacturerDto>(manufacturer);
        }
        //create
        public async Task<ManufacturerDto> CreateAsync(SaveManufacturerDto manufacturer)
        {
            var created = await _repository.AddAsync(_mapper.Map<Manufacturer>(manufacturer));
            return _mapper.Map<ManufacturerDto>(created);
        }
        //update
        public Task UpdateAsync(int id, SaveManufacturerDto manufacturer) =>
            _repository.UpdateAsync(id, _mapper.Map<Manufacturer>(manufacturer));
        //delete
        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
