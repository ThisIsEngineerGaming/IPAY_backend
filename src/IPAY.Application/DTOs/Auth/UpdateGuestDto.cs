using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Auth
{
    //banning/unbanning guest
    public class UpdateGuestDto
    {
        public string? Name { get; set; }
        public bool? IsBanned { get; set; }
    }
}
