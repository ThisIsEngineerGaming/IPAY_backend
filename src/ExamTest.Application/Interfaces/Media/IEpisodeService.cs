using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.DTOs.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface IEpisodeService
    {
        Task<IReadOnlyList<EpisodeDto>> GetAllAsync();
        Task<EpisodeDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<EpisodeDto>> GetBySeriesIdAsync(int seriesId);
        Task<EpisodeDto> CreateAsync(SaveEpisodeDto episode);
        Task UpdateAsync(int id, SaveEpisodeDto episode);
        Task DeleteAsync(int id);
    }
}
