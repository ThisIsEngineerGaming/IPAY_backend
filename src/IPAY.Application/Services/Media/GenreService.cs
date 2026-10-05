using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;

namespace IPAY.Application.Services.Media
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepo _repository;
        private readonly IMapper _mapper;

        public GenreService(IGenreRepo repository, IMapper mapper)
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

        // Atomic array updates in the repo: no read-modify-write, and a missing genre is a no-op.
        public Task AddSeriesToGenreAsync(int genreId, int seriesId) => _repository.AddSeriesAsync(genreId, seriesId);

        public Task AddFilmToGenreAsync(int genreId, int filmId) => _repository.AddFilmAsync(genreId, filmId);

        public Task RemoveSeriesFromGenreAsync(int genreId, int seriesId) => _repository.RemoveSeriesAsync(genreId, seriesId);

        public Task RemoveFilmFromGenreAsync(int genreId, int filmId) => _repository.RemoveFilmAsync(genreId, filmId);

        public Task ClearGenreAsync(int genreId) => _repository.ClearAsync(genreId);
    }
}
