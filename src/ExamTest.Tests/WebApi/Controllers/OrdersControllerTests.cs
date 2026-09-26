using ExamTest.WebApi.Controllers;
using NUnit.Framework;

namespace ExamTest.Tests.WebApi.Controllers
{
    // OrdersController is currently an empty class with no members, no route attributes and no
    // service dependency yet, so there is no request/response behaviour to exercise.
    // This fixture is a placeholder for when the endpoint is actually implemented.
    [TestFixture]
    public class OrdersControllerTests
    {
        [Test]
        [Ignore("OrdersController has no implementation yet - see ExamTest.WebApi/Controllers/OrdersController.cs")]
        public void PendingImplementation()
        {
            _ = new OrdersController();
        }
    }
}
