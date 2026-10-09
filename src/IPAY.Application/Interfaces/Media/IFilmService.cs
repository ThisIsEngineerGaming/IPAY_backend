using System.Collections.Generic;
using System.Threading.Tasks;
using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Interfaces.Media
{
    public interface IFilmService
    {
        Task<IReadOnlyList<FilmDto>> GetAllAsync();
        Task<IReadOnlyList<FilmDto>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir);
        Task<FilmDto?> GetByIdAsync(int id);
        Task<FilmDto> CreateAsync(SaveFilmDto film);
        Task UpdateAsync(int id, SaveFilmDto film);
        Task DeleteAsync(int id);
    }
}
