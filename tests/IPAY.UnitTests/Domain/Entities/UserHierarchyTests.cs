using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using NUnit.Framework;

namespace IPAY.UnitTests.Domain.Entities
{
    [TestFixture]
    public class UserHierarchyTests
    {
        [Test]
        public void Seller_IsACustomerWithSellerRole()
        {
            var seller = new Seller();

            Assert.That(seller, Is.InstanceOf<Customer>());
            Assert.That(seller, Is.InstanceOf<Guest>());
            Assert.That(seller.Role, Is.EqualTo(UserRole.Seller));
        }

        [Test]
        public void Customer_DefaultsToNotBanned()
        {
            Assert.That(new Customer().IsBanned, Is.False);
        }
    }
}
