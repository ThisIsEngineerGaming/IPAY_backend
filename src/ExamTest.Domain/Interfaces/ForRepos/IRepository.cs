using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamTest.Domain.Interfaces.ForRepos
{
    /// <summary>
    /// Generic CRUD contract. Implemented by ExamTest.Infastructure against Firebase
    /// (or any other store later) without the Domain/Application layers knowing the difference.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(int id, T entity);
        Task DeleteAsync(int id);
    }
}
