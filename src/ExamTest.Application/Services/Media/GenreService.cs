using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class GenreService : IGenreService
    {
        private readonly IRepository<Genre> _repository;

        public GenreService(IRepository<Genre> repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<Genre>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Genre?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<Genre> CreateAsync(Genre genre) => _repository.AddAsync(genre);

        public Task UpdateAsync(int id, Genre genre) => _repository.UpdateAsync(id, genre);

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

        public async Task AddSeriesToGenreAsync(int genreId, int seriesId)
        {
            var genre = await _repository.GetByIdAsync(genreId);
            if (genre is null) return;
            if (!genre.SerialIds.Contains(seriesId)) genre.SerialIds.Add(seriesId);
            await _repository.UpdateAsync(genreId, genre);
        }

        public async Task AddFilmToGenreAsync(int genreId, int filmId)
        {
            var genre = await _repository.GetByIdAsync(genreId);
            if (genre is null) return;
            if (!genre.FilmIds.Contains(filmId)) genre.FilmIds.Add(filmId);
            await _repository.UpdateAsync(genreId, genre);
        }

        public async Task RemoveSeriesFromGenreAsync(int genreId, int seriesId)
        {
            var genre = await _repository.GetByIdAsync(genreId);
            if (genre is null) return;
            genre.SerialIds.Remove(seriesId);
            await _repository.UpdateAsync(genreId, genre);
        }

        public async Task RemoveFilmFromGenreAsync(int genreId, int filmId)
        {
            var genre = await _repository.GetByIdAsync(genreId);
            if (genre is null) return;
            genre.FilmIds.Remove(filmId);
            await _repository.UpdateAsync(genreId, genre);
        }

        public async Task ClearGenreAsync(int genreId)
        {
            var genre = await _repository.GetByIdAsync(genreId);
            if (genre is null) return;
            genre.FilmIds.Clear();
            genre.SerialIds.Clear();
            await _repository.UpdateAsync(genreId, genre);
        }
    }
}
