using IPAY.Domain.Entities.Shop;

namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    public interface IOrderStatusHistoryRepository
    {
        Task<OrderStatusHistory> AddAsync(OrderStatusHistory entity);
        Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(int orderId);
    }
}
