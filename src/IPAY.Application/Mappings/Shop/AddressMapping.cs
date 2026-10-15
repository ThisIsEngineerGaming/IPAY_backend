using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Mappings.Shop
{
    using AutoMapper;
    using IPAY.Application.DTOs.Shop;
    using IPAY.Domain.Entities.Shop;
    using IPAY.Domain.Enums;

    public class OrderMapping : Profile
    {
        public OrderMapping()
        {
            // ===== Create =====
            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())      // обычно берём из токена
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Address, opt => opt.Ignore())     // если адрес не передаём
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore());      // цену берём из Product

            // ===== Read =====
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src =>
                    src.Items.Sum(i => i.Quantity * i.Price)))
                .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src =>
                    src.Items.Sum(i => i.Quantity)));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.Price));

            // ===== Address (на всякий случай) =====
            CreateMap<Address, AddressDto>().ReverseMap();

            // ===== Update Status =====
            // Обычно статус обновляют вручную в сервисе, а не через маппер
        }
    }
}
