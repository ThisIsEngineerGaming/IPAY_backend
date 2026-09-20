using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class SeriesService : ISeriesService
    {
        private readonly IRepository<Series> _repository;
        private readonly IMapper _mapper;

        public SeriesService(IRepository<Series> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<SeriesDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<SeriesDto>>(all);
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

        public Task UpdateAsync(int id, SaveSeriesDto series) =>
            _repository.UpdateAsync(id, _mapper.Map<Series>(series));

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
