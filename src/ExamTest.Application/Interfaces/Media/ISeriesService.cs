using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.DTOs.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface ISeriesService
    {
        Task<IReadOnlyList<SeriesDto>> GetAllAsync();
        Task<SeriesDto?> GetByIdAsync(int id);
        Task<SeriesDto> CreateAsync(SaveSeriesDto series);
        Task UpdateAsync(int id, SaveSeriesDto series);
        Task DeleteAsync(int id);
    }
}
