using IPAY.Domain.Enums;
using AutoMapper;
using IPAY.Application.DTOs.Auth;
using IPAY.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Mappings.Auth
{
    public class AuthMapping:Profile
    {
        public AuthMapping() {
            CreateMap<RegisterDto, Customer>()
              .ForMember(dest => dest.Role, opt => opt.MapFrom(_ => UserRole.Customer))
              .ForMember(dest => dest.Password, opt => opt.Ignore());

        }

    }
}
