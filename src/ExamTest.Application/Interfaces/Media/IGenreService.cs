using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.DTOs.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface IGenreService
    {
        Task<IReadOnlyList<GenreDto>> GetAllAsync();
        Task<GenreDto?> GetByIdAsync(int id);
        Task<GenreDto> CreateAsync(SaveGenreDto genre);
        Task UpdateAsync(int id, SaveGenreDto genre);
        Task DeleteAsync(int id);

        // Matches the overloaded addToGenre(serialId) / addToGenre(filmId) on the diagram.
        Task AddSeriesToGenreAsync(int genreId, int seriesId);
        Task AddFilmToGenreAsync(int genreId, int filmId);
        Task RemoveSeriesFromGenreAsync(int genreId, int seriesId);
        Task RemoveFilmFromGenreAsync(int genreId, int filmId);
        Task ClearGenreAsync(int genreId);
    }
}
