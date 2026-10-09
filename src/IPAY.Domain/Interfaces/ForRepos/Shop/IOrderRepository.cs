using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId);
        Task<Order?> GetByIdWithItemsAsync(int id); // с айтемами
    }
}
