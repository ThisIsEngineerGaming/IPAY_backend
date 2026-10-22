using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Shop
{
    //adress dto(making during registration order
    public class CreateAddressDto
    {
        public string Country { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
