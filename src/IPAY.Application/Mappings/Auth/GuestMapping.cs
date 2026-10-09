using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Mappings.Shop
{
    using AutoMapper;
    using global::IPAY.Application.DTOs.Auth;
    using global::IPAY.Domain.Entities.Users;
    using global::IPAY.Domain.Enums;



        public class GuestMapping : Profile
        {
            public GuestMapping()
            {


                // --- Guest ---

                // entity → ответ клиенту
                CreateMap<Guest, GuestDto>()
                    .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id ?? 0))
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name ?? "Guest"))
                    .ForMember(d => d.SessionKey, opt => opt.MapFrom(s => s.SessionKey ?? string.Empty));

                // создание сессии → entity
                CreateMap<GuestSessionDto, Guest>()
                    .ForMember(d => d.Id, opt => opt.Ignore())
                    .ForMember(d => d.Role, opt => opt.MapFrom(_ => UserRole.Guest))
                    .ForMember(d => d.IsBanned, opt => opt.MapFrom(_ => false))
                    .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                    .ForMember(d => d.Name, opt => opt.MapFrom(s =>
                        string.IsNullOrWhiteSpace(s.Name) ? "Guest" : s.Name.Trim()))
                    .ForMember(d => d.SessionKey, opt => opt.MapFrom(s =>
                        string.IsNullOrWhiteSpace(s.SessionKey)
                            ? Guid.NewGuid().ToString("N")
                            : s.SessionKey));

                // админский update → поверх существующей entity
                CreateMap<UpdateGuestDto, Guest>()
                    .ForMember(d => d.Id, opt => opt.Ignore())
                    .ForMember(d => d.Role, opt => opt.Ignore())
                    .ForMember(d => d.SessionKey, opt => opt.Ignore())
                    .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            }
        }
    }

