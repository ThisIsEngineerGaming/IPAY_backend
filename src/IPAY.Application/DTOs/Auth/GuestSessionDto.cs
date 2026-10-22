using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Auth
{
    //a key which allows to examine the info
    public class GuestSessionDto
    {
        public string? SessionKey { get; set; }
        public string? Name { get; set; }  // Guest
    }
}
