using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;

namespace IPAY.Application.Services.Media
{
    public class FilmService : IFilmService
    {
        private readonly IFilmRepo _repository;
        private readonly IMapper _mapper;

        public FilmService(IFilmRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<FilmDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<FilmDto>>(all);
        }

        public async Task<IReadOnlyList<FilmDto>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir)
        {
            limit = Math.Clamp(limit, 1, 50);
            var page = await _repository.GetPageAsync(limit, lastDocId, genreId, search, sortBy, sortDir);
            return _mapper.Map<List<FilmDto>>(page);
        }

        public async Task<FilmDto?> GetByIdAsync(int id)
        {
            var film = await _repository.GetByIdAsync(id);
            return film is null ? null : _mapper.Map<FilmDto>(film);
        }

        public async Task<FilmDto> CreateAsync(SaveFilmDto film)
        {
            var created = await _repository.AddAsync(_mapper.Map<Film>(film));
            return _mapper.Map<FilmDto>(created);
        }

        public async Task UpdateAsync(int id, SaveFilmDto film)
        {
            var entity = _mapper.Map<Film>(film);
            // The request has no CreatedAt; keep the original so "sort by date" stays meaningful.
            var existing = await _repository.GetByIdAsync(id);
            if (existing is not null) entity.CreatedAt = existing.CreatedAt;
            await _repository.UpdateAsync(id, entity);
        }

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
