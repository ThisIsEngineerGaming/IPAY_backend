using IPAY.Domain.Enums;

namespace IPAY.Domain.Entities.Users
{
    public class Seller : Customer
    {
        public Seller()
        {
            Role = UserRole.Seller;
        }
    }
}
