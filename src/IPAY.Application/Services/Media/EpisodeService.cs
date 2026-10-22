using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;

namespace IPAY.Application.Services.Media
{
    //episodes service
    public class EpisodeService : IEpisodeService
    {
        private readonly IEpisodeRepo _repository;
        private readonly IMapper _mapper;//using transfer dto-> object and object->dto

        public EpisodeService(IEpisodeRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        //get all
        public async Task<IReadOnlyList<EpisodeDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<EpisodeDto>>(all);
        }
        //get by id
        public async Task<EpisodeDto?> GetByIdAsync(int id)
        {
            var episode = await _repository.GetByIdAsync(id);
            return episode is null ? null : _mapper.Map<EpisodeDto>(episode);
        }
        //getting episode by series
        public async Task<IReadOnlyList<EpisodeDto>> GetBySeriesIdAsync(int seriesId)
        {
            var episodes = await _repository.GetBySeriesIdAsync(seriesId);
            return _mapper.Map<List<EpisodeDto>>(episodes);
        }
        //create episodes
        public async Task<EpisodeDto> CreateAsync(SaveEpisodeDto episode)
        {
            var created = await _repository.AddAsync(_mapper.Map<Episode>(episode));
            return _mapper.Map<EpisodeDto>(created);
        }
        //update episode
        public Task UpdateAsync(int id, SaveEpisodeDto episode) =>
            _repository.UpdateAsync(id, _mapper.Map<Episode>(episode));
        //delete episode
        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
