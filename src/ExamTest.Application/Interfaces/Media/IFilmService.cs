using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.DTOs.Media;

namespace ExamTest.Application.Interfaces.Media
{
    public interface IFilmService
    {
        Task<IReadOnlyList<FilmDto>> GetAllAsync();
        Task<FilmDto?> GetByIdAsync(int id);
        Task<FilmDto> CreateAsync(SaveFilmDto film);
        Task UpdateAsync(int id, SaveFilmDto film);
        Task DeleteAsync(int id);
    }
}
