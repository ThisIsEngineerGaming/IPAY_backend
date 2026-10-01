using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPAY.Domain.Interfaces.ForRepos
{
    /// <summary>
    /// Generic CRUD contract. Implemented by IPAY.Infrastructure against Firebase
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
