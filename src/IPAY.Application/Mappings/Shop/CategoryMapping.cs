using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Domain.Entities.Shop;

namespace IPAY.Application.Mappings.Shop
{
    public class CategoryMapping : Profile
    {
        public CategoryMapping()
        {
            // Entity → response DTO
            CreateMap<Category, CategoryDto>();

            // Request DTO → Entity (Id is assigned by the repository / route, never by the client)
            CreateMap<SaveCategoryDto, Category>();
        }
    }
}
