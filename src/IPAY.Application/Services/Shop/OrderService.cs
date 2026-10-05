using AutoMapper;
using FluentValidation;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Services.Shop
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateOrderDto> _createOrderValidator;
        private readonly IValidator<CreateOrderItemDto> _createOrderItemValidator;
        private readonly IValidator<UpdateOrderStatusDto> _updateStatusValidator;

        private readonly IProductRepo _productRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IAddressRepository addressRepository,
            IMapper mapper,
            IValidator<CreateOrderDto> createOrderValidator,
            IValidator<CreateOrderItemDto> createOrderItemValidator,
            IValidator<UpdateOrderStatusDto> updateStatusValidator, IProductRepo productRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _addressRepository = addressRepository;
            _mapper = mapper;
            _createOrderValidator = createOrderValidator;
            _createOrderItemValidator = createOrderItemValidator;
            _updateStatusValidator = updateStatusValidator;
            _productRepository = productRepository;
        }

        // ==================== ORDER ====================

        public async Task<IReadOnlyList<OrderDto>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();

            foreach (var order in orders)
            {
                order.Items = (await _orderItemRepository.GetByOrderIdAsync(order.Id)).ToList();
            }

            return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null) return null;

            order.Items = (await _orderItemRepository.GetByOrderIdAsync(id)).ToList();
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IReadOnlyList<OrderDto>> GetByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);

            foreach (var order in orders)
            {
                order.Items = (await _orderItemRepository.GetByOrderIdAsync(order.Id)).ToList();
            }

            return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto dto, int userId)
        {
            await _createOrderValidator.ValidateAndThrowAsync(dto);

            // 1. Создаём адрес
            var address = new Address
            {
                Country = dto.Address.Country,
                Street = dto.Address.Street,
                City = dto.Address.City,
                UserId = userId
            };
            address = await _addressRepository.AddAsync(address);

            // 2. Создаём заказ
            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Address = address,
                Items = new List<OrderItem>()
            };
            order = await _orderRepository.AddAsync(order);

            // 3. Создаём айтемы + берём цену из продукта
            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepository.GetProductByIdAsync(itemDto.ProductId)
                    ?? throw new KeyNotFoundException($"Product with id {itemDto.ProductId} not found");

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    Price = product.Price  ,
                    ProductName=product.Name// ← вот здесь берём цену
                                                   // если есть скидка: Price = product.Price * (1 - product.Discount / 100)
                };

                orderItem = await _orderItemRepository.AddAsync(orderItem);
                order.Items.Add(orderItem);
            }

            await _orderRepository.UpdateAsync(order.Id, order);

            return _mapper.Map<OrderDto>(order);
        }

        public async Task UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            await _updateStatusValidator.ValidateAndThrowAsync(dto);

            var order = await _orderRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Order with id {id} not found");

            order.Status = dto.Status;
            await _orderRepository.UpdateAsync(id, order);
        }

        public async Task DeleteAsync(int id)
        {
            var items = await _orderItemRepository.GetByOrderIdAsync(id);
            foreach (var item in items)
            {
                await _orderItemRepository.DeleteAsync(item.Id);
            }

            await _orderRepository.DeleteAsync(id);
        }

        // ==================== ORDER ITEMS ====================

        public async Task<OrderItemDto> AddItemAsync(int orderId, CreateOrderItemDto dto)
        {
            await _createOrderItemValidator.ValidateAndThrowAsync(dto);

            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException($"Order with id {orderId} not found");

            var product = await _productRepository.GetProductByIdAsync(dto.ProductId)
                ?? throw new KeyNotFoundException($"Product with id {dto.ProductId} not found");

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = product.Price,
                ProductName = product.Name,
            };

            orderItem = await _orderItemRepository.AddAsync(orderItem);

            order.Items.Add(orderItem);
            await _orderRepository.UpdateAsync(orderId, order);

            return _mapper.Map<OrderItemDto>(orderItem);
        }

        public async Task UpdateItemAsync(int orderId, int itemId, CreateOrderItemDto dto)
        {
            await _createOrderItemValidator.ValidateAndThrowAsync(dto);

            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException($"Order with id {orderId} not found");

            var item = order.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException($"OrderItem with id {itemId} not found");

            item.ProductId = dto.ProductId;
            item.Quantity = dto.Quantity;

            await _orderItemRepository.UpdateAsync(itemId, item);
            await _orderRepository.UpdateAsync(orderId, order);
        }

        public async Task DeleteItemAsync(int orderId, int itemId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException($"Order with id {orderId} not found");

            var item = order.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException($"OrderItem with id {itemId} not found");

            order.Items.Remove(item);

            await _orderItemRepository.DeleteAsync(itemId);
            await _orderRepository.UpdateAsync(orderId, order);
        }
    }
}
