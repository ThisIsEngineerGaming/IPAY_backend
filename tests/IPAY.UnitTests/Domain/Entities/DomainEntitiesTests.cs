using IPAY.Domain.Entities.Auth;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using NUnit.Framework;

namespace IPAY.UnitTests.Domain.Entities
{
    [TestFixture]
    public class DomainEntitiesTests
    {
        // ---------- Product pricing ----------

        [Test]
        public void Product_GetFinalPrice_UsesTheDiscountWhenItIsLowerThanTheListPrice()
        {
            var product = new Product { Price = 100, DiscountedPrice = 80 };

            Assert.That(product.GetFinalPrice(), Is.EqualTo(80));
        }

        [TestCase(100, 0)]
        [TestCase(100, -5)]
        [TestCase(100, 100)]
        [TestCase(100, 150)]
        public void Product_GetFinalPrice_IgnoresMissingOrNonsenseDiscounts(double price, double discounted)
        {
            var product = new Product { Price = price, DiscountedPrice = discounted };

            Assert.That(product.GetFinalPrice(), Is.EqualTo(price));
        }

        [Test]
        public void Product_GetDiscountPercent_IsThePercentageOffTheListPrice()
        {
            var product = new Product { Price = 200, DiscountedPrice = 150 };

            Assert.That(product.GetDiscountPercent(), Is.EqualTo(25).Within(0.0001));
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Product_GetDiscountPercent_IsZeroWhenThereIsNoValidPrice(double price)
        {
            var product = new Product { Price = price, DiscountedPrice = 5 };

            Assert.That(product.GetDiscountPercent(), Is.EqualTo(0));
        }

        // ---------- RefreshToken ----------

        [Test]
        public void RefreshToken_IsActive_WhenNotRevokedAndNotExpired()
        {
            var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };

            Assert.That(token.IsActive, Is.True);
        }

        [Test]
        public void RefreshToken_IsNotActive_WhenRevoked()
        {
            var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddMinutes(5), IsRevoked = true };

            Assert.That(token.IsActive, Is.False);
        }

        [Test]
        public void RefreshToken_IsNotActive_WhenExpired()
        {
            var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };

            Assert.That(token.IsActive, Is.False);
        }

        // ---------- users ----------

        [Test]
        public void Guest_StartsAsAnUnbannedGuestNamedGuest()
        {
            var guest = new Guest();

            Assert.That(guest.Role, Is.EqualTo(UserRole.Guest));
            Assert.That(guest.Name, Is.EqualTo("Guest"));
            Assert.That(guest.IsBanned, Is.False);
            Assert.That(guest.Id, Is.Null);
        }

        [Test]
        public void Customer_StartsWithTheCustomerRoleAndNoVerificationStatus()
        {
            var customer = new Customer();

            Assert.That(customer.Role, Is.EqualTo(UserRole.Customer));
            Assert.That(customer.EmailVerified, Is.Null);
            Assert.That(customer.Phone, Is.Null);
        }

        [Test]
        public void SellerRequest_StartsPending()
        {
            var request = new SellerRequest();

            Assert.That(request.Status, Is.EqualTo(SellerRequestStatus.Pending));
            Assert.That(request.ReviewedAt, Is.Null);
        }

        // ---------- enums: the numbers are stored in the database / JWTs, so they must not shift ----------

        [Test]
        public void UserRole_KeepsItsNumericValues()
        {
            Assert.That((int)UserRole.Guest, Is.EqualTo(0));
            Assert.That((int)UserRole.Customer, Is.EqualTo(1));
            Assert.That((int)UserRole.Seller, Is.EqualTo(2));
            Assert.That((int)UserRole.Admin, Is.EqualTo(3));
            Assert.That((int)UserRole.Moderator, Is.EqualTo(4));
        }

        [Test]
        public void OrderStatus_KeepsItsNumericValues()
        {
            Assert.That((int)OrderStatus.Pending, Is.EqualTo(0));
            Assert.That((int)OrderStatus.Paid, Is.EqualTo(1));
            Assert.That((int)OrderStatus.Shipped, Is.EqualTo(2));
            Assert.That((int)OrderStatus.Delivered, Is.EqualTo(3));
            Assert.That((int)OrderStatus.Cancelled, Is.EqualTo(4));
        }

        [Test]
        public void PaymentAndSellerRequestStatuses_KeepTheirNumericValues()
        {
            Assert.That((int)PaymentStatus.Success, Is.EqualTo(1));
            Assert.That((int)PaymentStatus.Failed, Is.EqualTo(2));
            Assert.That((int)SellerRequestStatus.Approved, Is.EqualTo(1));
            Assert.That((int)SellerRequestStatus.Rejected, Is.EqualTo(2));
        }
    }
}
