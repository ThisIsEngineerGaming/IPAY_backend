using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Domain.Entities.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface IFilmService
    {
        Task<IReadOnlyList<Film>> GetAllAsync();
        Task<Film?> GetByIdAsync(int id);
        Task<Film> CreateAsync(Film film);
        Task UpdateAsync(int id, Film film);
        Task DeleteAsync(int id);
    }
}
