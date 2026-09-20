using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class EpisodeService : IEpisodeService
    {
        private readonly IRepository<Episode> _repository;
        private readonly IMapper _mapper;

        public EpisodeService(IRepository<Episode> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<EpisodeDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<EpisodeDto>>(all);
        }

        public async Task<EpisodeDto?> GetByIdAsync(int id)
        {
            var episode = await _repository.GetByIdAsync(id);
            return episode is null ? null : _mapper.Map<EpisodeDto>(episode);
        }

        public async Task<IReadOnlyList<EpisodeDto>> GetBySeriesIdAsync(int seriesId)
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<EpisodeDto>>(all.Where(e => e.SerialId == seriesId).ToList());
        }

        public async Task<EpisodeDto> CreateAsync(SaveEpisodeDto episode)
        {
            var created = await _repository.AddAsync(_mapper.Map<Episode>(episode));
            return _mapper.Map<EpisodeDto>(created);
        }

        public Task UpdateAsync(int id, SaveEpisodeDto episode) =>
            _repository.UpdateAsync(id, _mapper.Map<Episode>(episode));

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
