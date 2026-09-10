using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;

namespace ExamTest.Application.Services.Media
{
    public class FilmService : IFilmService
    {
        private readonly IRepository<Film> _repository;

        public FilmService(IRepository<Film> repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<Film>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Film?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<Film> CreateAsync(Film film) => _repository.AddAsync(film);

        public Task UpdateAsync(int id, Film film) => _repository.UpdateAsync(id, film);

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
