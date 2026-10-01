using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Domain.Entities.Shop;

namespace IPAY.Application.Mappings.Shop
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
