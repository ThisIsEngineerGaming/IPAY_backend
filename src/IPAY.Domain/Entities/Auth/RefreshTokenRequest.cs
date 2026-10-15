using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Entities.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
