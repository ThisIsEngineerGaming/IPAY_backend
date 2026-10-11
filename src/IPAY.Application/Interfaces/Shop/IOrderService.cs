using IPAY.Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Interfaces.Shop
{
    public enum CancelOrderResult
    {
        Cancelled,
        NotFound,       // no such order, or it belongs to someone else (deliberately indistinguishable)
        NotCancellable  // already on its way or delivered
    }

    public record CancelOrderOutcome(CancelOrderResult Result, OrderDto? Order = null);

    public interface IOrderService
    {
        // Order
        Task<IReadOnlyList<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<OrderDto>> GetByUserIdAsync(int userId);
        Task<OrderDto> CreateAsync(CreateOrderDto dto, int userId);
        Task UpdateStatusAsync(int id, UpdateOrderStatusDto dto);

        /// <summary>A customer cancelling their OWN order. Only allowed before it has shipped.</summary>
        Task<CancelOrderOutcome> CancelAsync(int orderId, int userId);
        Task DeleteAsync(int id);

        Task<IReadOnlyList<OrderStatusHistoryDto>> GetStatusHistoryAsync(int orderId);

        // OrderItems (внутри заказа)
        Task<OrderItemDto> AddItemAsync(int orderId, CreateOrderItemDto dto);
        Task UpdateItemAsync(int orderId, int itemId, CreateOrderItemDto dto);
        Task DeleteItemAsync(int orderId, int itemId);
    }
}
