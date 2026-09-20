using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ExamTest.Application.DTOs.Media;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class FilmService : IFilmService
    {
        private readonly IRepository<Film> _repository;
        private readonly IMapper _mapper;

        public FilmService(IRepository<Film> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<FilmDto>> GetAllAsync()
        {
            var all = await _repository.GetAllAsync();
            return _mapper.Map<List<FilmDto>>(all);
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

        public Task UpdateAsync(int id, SaveFilmDto film) =>
            _repository.UpdateAsync(id, _mapper.Map<Film>(film));

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
