using AutoMapper;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.Mappings.Shop
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
