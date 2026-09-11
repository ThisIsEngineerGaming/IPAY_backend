using ExamTest.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.Interfaces.Auth
{
   public interface IUser<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> CreateAsync(T user);
        Task UpdateAsync(int id, T user);
        Task DeleteAsync(int id);
    }
}
