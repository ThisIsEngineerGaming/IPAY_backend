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

        /// <summary>
        /// One-time email confirmation, stored on the account so it is never asked again.
        /// null  = account created before email verification existed (grandfathered: never blocked,
        ///         and no emailed sign-in code, since its address was never confirmed)
        /// false = just registered, still has to click the link in the verification email
        /// true  = confirmed (also set for accounts created through Google sign-in)
        /// </summary>
        public bool? EmailVerified { get; set; }
    }
}
