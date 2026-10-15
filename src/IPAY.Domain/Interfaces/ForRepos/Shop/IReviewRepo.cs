using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    public interface IReviewRepository
    {
        Task<Review> AddAsync(Review entity);
        Task<Review?> GetByIdAsync(int id);
        Task<IReadOnlyList<Review>> GetByProductIdAsync(int productId);
        Task DeleteAsync(int id);
    }
}
