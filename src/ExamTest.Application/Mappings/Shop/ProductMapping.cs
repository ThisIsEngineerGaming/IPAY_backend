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
        public ProductMapping() {
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
        }

    }
}
