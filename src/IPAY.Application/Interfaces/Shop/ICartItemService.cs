using IPAY.Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Text;


namespace IPAY.Application.Interfaces.Shop
    {
        public interface ICartItemService
        {
            Task<CartItemDto?> GetByIdAsync(int userId, int id);
            Task<CartItemDto> AddAsync(int userId, CreateCartItemDto dto);
            Task<CartItemDto?> UpdateAsync(int userId, int id, CreateCartItemDto dto);
            Task<bool> DeleteAsync(int userId, int id);
        }
    }

