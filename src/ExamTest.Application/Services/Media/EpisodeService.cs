using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class EpisodeService : IEpisodeService
    {
        private readonly IRepository<Episode> _repository;

        public EpisodeService(IRepository<Episode> repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<Episode>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Episode?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public async Task<IReadOnlyList<Episode>> GetBySeriesIdAsync(int seriesId)
        {
            var all = await _repository.GetAllAsync();
            return all.Where(e => e.SerialId == seriesId).ToList();
        }

        public Task<Episode> CreateAsync(Episode episode) => _repository.AddAsync(episode);

        public Task UpdateAsync(int id, Episode episode) => _repository.UpdateAsync(id, episode);

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
