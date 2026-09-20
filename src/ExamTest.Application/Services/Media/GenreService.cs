using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class GenreService : IGenreService
    {
        private readonly IRepository<Genre> _repository;
        private readonly IMapper _mapper;

        public GenreService(IRepository<Genre> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<GenreDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<GenreDto>>(all);
        }

        public async Task<GenreDto?> GetByIdAsync(int id)
        {
            var genre = await _repository.GetByIdAsync(id);
            return genre is null ? null : _mapper.Map<GenreDto>(genre);
        }

        public async Task<GenreDto> CreateAsync(SaveGenreDto genre)
        {
            var created = await _repository.AddAsync(_mapper.Map<Genre>(genre));
            return _mapper.Map<GenreDto>(created);
        }

        public Task UpdateAsync(int id, SaveGenreDto genre) =>
            _repository.UpdateAsync(id, _mapper.Map<Genre>(genre));

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
