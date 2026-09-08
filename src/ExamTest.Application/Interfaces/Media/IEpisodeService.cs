using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Domain.Entities.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface IEpisodeService
    {
        Task<IReadOnlyList<Episode>> GetAllAsync();
        Task<Episode?> GetByIdAsync(int id);
        Task<IReadOnlyList<Episode>> GetBySeriesIdAsync(int seriesId);
        Task<Episode> CreateAsync(Episode episode);
        Task UpdateAsync(int id, Episode episode);
        Task DeleteAsync(int id);
    }
}
