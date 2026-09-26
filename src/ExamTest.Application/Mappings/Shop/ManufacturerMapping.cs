using AutoMapper;
using ExamTest.Application.DTOs.Shop;
using ExamTest.Domain.Entities.Shop;

namespace ExamTest.Application.Mappings.Shop
{
    public class ManufacturerMapping : Profile
    {
        public ManufacturerMapping()
        {
            // Entity → response DTO
            CreateMap<Manufacturer, ManufacturerDto>();

            // Request DTO → Entity (Id is assigned by the repository / route, never by the client)
            CreateMap<SaveManufacturerDto, Manufacturer>();
        }
    }
}
