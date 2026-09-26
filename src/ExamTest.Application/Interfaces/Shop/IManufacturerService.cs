using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.DTOs.Shop;

namespace ExamTest.Application.Interfaces.Shop
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
