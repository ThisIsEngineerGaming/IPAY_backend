using System.Collections.Generic;
using System.Threading.Tasks;
using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Interfaces.Media
{
    public interface ISeriesService
    {
        Task<IReadOnlyList<SeriesDto>> GetAllAsync();
        Task<IReadOnlyList<SeriesDto>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir);
        Task<SeriesDto?> GetByIdAsync(int id);
        Task<SeriesDto> CreateAsync(SaveSeriesDto series);
        Task UpdateAsync(int id, SaveSeriesDto series);
        Task DeleteAsync(int id);
    }
}
