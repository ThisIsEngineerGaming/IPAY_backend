using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;

namespace IPAY.Application.Services.Media
{
    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepo _repository;
        private readonly IMapper _mapper;

        public SeriesService(ISeriesRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<SeriesDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<SeriesDto>>(all);
        }

        public async Task<IReadOnlyList<SeriesDto>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir)
        {
            limit = Math.Clamp(limit, 1, 50);
            var page = await _repository.GetPageAsync(limit, lastDocId, genreId, search, sortBy, sortDir);
            return _mapper.Map<List<SeriesDto>>(page);
        }

        public async Task<SeriesDto?> GetByIdAsync(int id)
        {
            var series = await _repository.GetByIdAsync(id);
            return series is null ? null : _mapper.Map<SeriesDto>(series);
        }

        public async Task<SeriesDto> CreateAsync(SaveSeriesDto series)
        {
            var created = await _repository.AddAsync(_mapper.Map<Series>(series));
            return _mapper.Map<SeriesDto>(created);
        }

        public async Task UpdateAsync(int id, SaveSeriesDto series)
        {
            var entity = _mapper.Map<Series>(series);
            // The request has no CreatedAt; keep the original so "sort by date" stays meaningful.
            var existing = await _repository.GetByIdAsync(id);
            if (existing is not null) entity.CreatedAt = existing.CreatedAt;
            await _repository.UpdateAsync(id, entity);
        }

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
