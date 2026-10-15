using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Auth
{
    public class GuestSessionDto
    {
        public string? SessionKey { get; set; }
        public string? Name { get; set; }  // опционально, по умолчанию "Guest"
    }
}
