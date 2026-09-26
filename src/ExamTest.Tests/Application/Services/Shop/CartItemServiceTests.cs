using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Shop
{
    // CartItemService is still an internal, empty scaffold class in ExamTest.Application (no members,
    // not registered behind an interface yet), so there is nothing behavioural to assert here.
    // This fixture is kept as a placeholder - ported over from the master branch - so that
    // real cases can be dropped in as soon as CartItemService grows an implementation and a public
    // interface to depend on.
    [TestFixture]
    public class CartItemServiceTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Ignore("CartItemService has no implementation yet - see ExamTest.Application/Services/Shop/CartItemService.cs")]
        public void PendingImplementation()
        {
        }
    }
}
