using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Domain.Entities.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface ISeriesService
    {
        Task<IReadOnlyList<Series>> GetAllAsync();
        Task<Series?> GetByIdAsync(int id);
        Task<Series> CreateAsync(Series series);
        Task UpdateAsync(int id, Series series);
        Task DeleteAsync(int id);
    }
}
