using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Domain.Entities.Shop;

namespace IPAY.Application.Mapping
{
    public class CartMapping : Profile
    {
        public CartMapping()
        {
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.TotalItemsCount, opt => opt.Ignore());

            CreateMap<CreateCartItemDto, CartItem>();
        }
    }
}
