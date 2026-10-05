using IPAY.Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Interfaces.Shop
{
    public interface IOrderService
    {
        // Order
        Task<IReadOnlyList<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<OrderDto>> GetByUserIdAsync(int userId);
        Task<OrderDto> CreateAsync(CreateOrderDto dto, int userId);
        Task UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
        Task DeleteAsync(int id);

        // OrderItems (внутри заказа)
        Task<OrderItemDto> AddItemAsync(int orderId, CreateOrderItemDto dto);
        Task UpdateItemAsync(int orderId, int itemId, CreateOrderItemDto dto);
        Task DeleteItemAsync(int orderId, int itemId);
    }
}
