using System.Collections.Generic;
using System.Threading.Tasks;
using IPAY.Application.DTOs.Shop;

namespace IPAY.Application.Interfaces.Shop
{
    public interface IManufacturerService
    {
        Task<IReadOnlyList<ManufacturerDto>> GetAllAsync();
        Task<ManufacturerDto?> GetByIdAsync(int id);
        Task<ManufacturerDto> CreateAsync(SaveManufacturerDto manufacturer);
        Task UpdateAsync(int id, SaveManufacturerDto manufacturer);
        Task DeleteAsync(int id);
    }
}
