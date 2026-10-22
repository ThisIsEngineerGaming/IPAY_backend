using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    //order items repository(also inherited base repo)
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(int orderId);
    }
}
