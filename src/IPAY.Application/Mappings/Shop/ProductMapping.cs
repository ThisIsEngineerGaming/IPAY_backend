using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Mappings.Shop
{
    public class ProductMapping : Profile
    {
        public ProductMapping()
        {
            // Entity → DTO (со скидкой)
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.DiscountPercent,
                           opt => opt.MapFrom(src => src.GetDiscountPercent()));
            // Entity → DTO (со скидкой)
            CreateMap<Product, CreateAdminProductDto>()
                .ForMember(dest => dest.DiscountPercent,
                           opt => opt.MapFrom(src => src.GetDiscountPercent()));
            // Entity → DTO (со скидкой)
            CreateMap<Product, UpdateAdminProductDto>()
                .ForMember(dest => dest.DiscountPercent,
                           opt => opt.MapFrom(src => src.GetDiscountPercent()));

            // DTO → Entity
            CreateMap<ProductDto, Product>();
            CreateMap<CreateAdminProductDto, Product>();
            CreateMap<UpdateAdminProductDto, Product>();
        }
    }
}
