using IPAY.Domain.Entities.Users;
using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Interfaces.Auth
{
   public interface IUser<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T?> CreateAsync(T user);
        Task UpdateAsync(int id, T user);
        Task DeleteAsync(int id);

         Task<T?> GetByEmail(string email);
    }
}
