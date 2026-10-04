using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos;
using  IPAY.Domain.Interfaces.ForRepos.Shop;

namespace IPAY.Application.Services.Shop
{
    public class CartItemService : ICartItemService
    {
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IProductRepo _productRepository;
        private readonly IMapper _mapper;

        public CartItemService(
            IRepository<CartItem> cartItemRepository,
            IRepository<Cart> cartRepository,
            IProductRepo productRepository,
            IMapper mapper)
        {
            _cartItemRepository = cartItemRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        private async Task<Cart?> GetUserCartAsync(int userId)
        {
            var carts = await _cartRepository.GetAllAsync();
            return carts.FirstOrDefault(c => c.UserId == userId);
        }

        private async Task<bool> ItemBelongsToUserAsync(int userId, int itemId)
        {
            var cart = await GetUserCartAsync(userId);
            return cart is not null && cart.Items.Any(i => i.Id == itemId);
        }

        private async Task<CartItemDto> BuildItemDtoAsync(CartItem item)
        {
            var dto = _mapper.Map<CartItemDto>(item);

            var product = await _productRepository.GetProductByIdAsync(item.ProductId);
            if (product is not null)
            {
                dto.ProductName = product.Name;
                dto.ImageUrl = product.ImageUrl;
                dto.Price = product.Price;
            }

            return dto;
        }

        public async Task<CartItemDto?> GetByIdAsync(int userId, int id)
        {
            if (!await ItemBelongsToUserAsync(userId, id))
                return null;

            var item = await _cartItemRepository.GetByIdAsync(id);
            return item is null ? null : await BuildItemDtoAsync(item);
        }

        public async Task<CartItemDto> AddAsync(int userId, CreateCartItemDto dto)
        {
            var cart = await GetUserCartAsync(userId);
            if (cart is null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                cart = await _cartRepository.AddAsync(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existingItem is not null)
            {
                existingItem.Quantity += dto.Quantity;
                await _cartItemRepository.UpdateAsync(existingItem.Id, existingItem);
                await _cartRepository.UpdateAsync(cart.Id, cart);

                return await BuildItemDtoAsync(existingItem);
            }

            var item = _mapper.Map<CartItem>(dto);
            var newId = cart.Items.Count > 0 ? cart.Items.Max(i => i.Id) + 1 : 1;
            item.Id = newId;
            item.CartId = cart.Id;

            var created = await _cartItemRepository.AddAsync(item);

            cart.Items.Add(created);
            await _cartRepository.UpdateAsync(cart.Id, cart);

            return await BuildItemDtoAsync(created);
        }

        public async Task<CartItemDto?> UpdateAsync(int userId, int id, CreateCartItemDto dto)
        {
            if (!await ItemBelongsToUserAsync(userId, id))
                return null;

            var existing = await _cartItemRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.Quantity = dto.Quantity;

            await _cartItemRepository.UpdateAsync(id, existing);
            return await BuildItemDtoAsync(existing);
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            if (!await ItemBelongsToUserAsync(userId, id))
                return false;

            await _cartItemRepository.DeleteAsync(id);
            return true;
        }
    }
}
