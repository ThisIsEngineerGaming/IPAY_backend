using System;
using System.Collections.Generic;
using System.Text;


namespace IPAY.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Cancelled = 3
    }
}
