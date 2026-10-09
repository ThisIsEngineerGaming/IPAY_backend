using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Services.Shop
{
    using AutoMapper;
    using global::IPAY.Application.DTOs.Shop;
    using global::IPAY.Application.Interfaces.Shop;
    using global::IPAY.Domain.Entities.Shop;
    using global::IPAY.Domain.Interfaces.ForRepos;
    using global::IPAY.Domain.Interfaces.ForRepos.Shop;

    namespace IPAY.Application.Services.Shop
    {
        public class CartService : ICartService
        {
            private readonly IRepository<Cart> _cartRepository;
            private readonly IProductRepo _productRepository;
            private readonly IMapper _mapper;

            public CartService(
                IRepository<Cart> cartRepository,
                IProductRepo productRepository,
                IMapper mapper)
            {
                _cartRepository = cartRepository;
                _productRepository = productRepository;
                _mapper = mapper;
            }

            private async Task<Cart?> FindByUserIdAsync(int userId)
            {
                var carts = await _cartRepository.GetAllAsync();
                return carts.FirstOrDefault(c => c.UserId == userId);
            }

            private async Task<CartDto> BuildCartDtoAsync(Cart cart)
            {
                var dto = _mapper.Map<CartDto>(cart);

                foreach (var itemDto in dto.Items)
                {
                    var product = await _productRepository.GetProductByIdAsync(itemDto.ProductId);
                    if (product is not null)
                    {
                        itemDto.ProductName = product.Name;
                        itemDto.ImageUrl = product.ImageUrl;
                        itemDto.Price = product.Price;
                    }
                }

                return dto;
            }

            public async Task<CartDto?> GetByUserIdAsync(int userId)
            {
                var cart = await FindByUserIdAsync(userId);
                return cart is null ? null : await BuildCartDtoAsync(cart);
            }

            public async Task<CartDto> AddItemAsync(int userId, CreateCartItemDto dto)
            {
                var cart = await FindByUserIdAsync(userId);

                if (cart is null)
                {
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                    cart = await _cartRepository.AddAsync(cart);
                }

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
                if (existingItem is not null)
                {
                    existingItem.Quantity += dto.Quantity;
                }
                else
                {
                    var newId = cart.Items.Count > 0 ? cart.Items.Max(i => i.Id) + 1 : 1;
                    cart.Items.Add(new CartItem
                    {
                        Id = newId,
                        CartId = cart.Id,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity
                    });
                }

                await _cartRepository.UpdateAsync(cart.Id, cart);

                return await BuildCartDtoAsync(cart);
            }

            public async Task<CartDto?> UpdateItemQuantityAsync(int userId, int itemId, CreateCartItemDto dto)
            {
                var cart = await FindByUserIdAsync(userId);
                if (cart is null) return null;

                var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
                if (item is null) return null;

                if (dto.Quantity <= 0)
                    cart.Items.Remove(item);
                else
                    item.Quantity = dto.Quantity;

                await _cartRepository.UpdateAsync(cart.Id, cart);

                return await BuildCartDtoAsync(cart);
            }

            public async Task<CartDto?> RemoveItemAsync(int userId, int itemId)
            {
                var cart = await FindByUserIdAsync(userId);
                if (cart is null) return null;

                var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
                if (item is null) return null;

                cart.Items.Remove(item);
                await _cartRepository.UpdateAsync(cart.Id, cart);

                return await BuildCartDtoAsync(cart);
            }

            public async Task ClearCartAsync(int userId)
            {
                var cart = await FindByUserIdAsync(userId);
                if (cart is null) return;

                cart.Items.Clear();
                await _cartRepository.UpdateAsync(cart.Id, cart);
            }
        }
    }
}