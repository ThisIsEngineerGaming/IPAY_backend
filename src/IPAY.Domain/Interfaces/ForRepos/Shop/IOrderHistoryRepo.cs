using IPAY.Domain.Entities.Shop;

namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    //repository related with saving a history of changing orders status
    public interface IOrderStatusHistoryRepository
    {
        Task<OrderStatusHistory> AddAsync(OrderStatusHistory entity);
        Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(int orderId);
    }
}
