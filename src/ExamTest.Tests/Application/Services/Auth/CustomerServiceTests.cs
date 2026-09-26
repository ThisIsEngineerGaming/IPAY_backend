using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Auth
{
    // ExamTest.Application/Services/Auth/CustomerService.cs currently declares an empty,
    // internal "RegisterService" class with no members and no interface - it looks like
    // scaffolding for customer-facing registration that hasn't been built out yet (the
    // seller-focused flow lives in RegisterDtoService/SellerService instead, which are
    // already covered by their own test fixtures). This placeholder is kept so a real
    // fixture can be dropped in once that class is implemented.
    [TestFixture]
    public class CustomerServiceTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Ignore("CustomerService has no implementation yet - see ExamTest.Application/Services/Auth/CustomerService.cs")]
        public void PendingImplementation()
        {
        }
    }
}
