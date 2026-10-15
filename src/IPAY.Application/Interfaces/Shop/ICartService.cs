using IPAY.Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Interfaces.Shop
{

        public interface ICartService
        {
            Task<CartDto?> GetByUserIdAsync(int userId);
            Task<CartDto> AddItemAsync(int userId, CreateCartItemDto dto);
            Task<CartDto?> UpdateItemQuantityAsync(int userId, int itemId, CreateCartItemDto dto);
            Task<CartDto?> RemoveItemAsync(int userId, int itemId);
            Task ClearCartAsync(int userId);
        }
 }
