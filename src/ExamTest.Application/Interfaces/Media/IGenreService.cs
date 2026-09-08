using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Domain.Entities.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface IGenreService
    {
        Task<IReadOnlyList<Genre>> GetAllAsync();
        Task<Genre?> GetByIdAsync(int id);
        Task<Genre> CreateAsync(Genre genre);
        Task UpdateAsync(int id, Genre genre);
        Task DeleteAsync(int id);

        // Matches the overloaded addToGenre(serialId) / addToGenre(filmId) on the diagram.
        Task AddSeriesToGenreAsync(int genreId, int seriesId);
        Task AddFilmToGenreAsync(int genreId, int filmId);
        Task RemoveSeriesFromGenreAsync(int genreId, int seriesId);
        Task RemoveFilmFromGenreAsync(int genreId, int filmId);
        Task ClearGenreAsync(int genreId);
    }
}
