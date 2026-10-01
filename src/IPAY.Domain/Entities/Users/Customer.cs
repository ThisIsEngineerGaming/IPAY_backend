using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Entities.Users
{
    public  class Customer:Guest
    {
        public string? Email { get; set; } = string.Empty;

        public string? Password { get; set; } = string.Empty;



        public bool IsBanned { get; set; } = false;
    }
}
