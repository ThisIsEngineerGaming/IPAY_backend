using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class SeriesService : ISeriesService
    {
        private readonly IRepository<Series> _repository;

        public SeriesService(IRepository<Series> repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<Series>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Series?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<Series> CreateAsync(Series series) => _repository.AddAsync(series);

        public Task UpdateAsync(int id, Series series) => _repository.UpdateAsync(id, series);

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
