using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class SessionControllerTests
    {
        [Test]
        public void Get_ReturnsNoContent()
        {
            Assert.That(new SessionController().Get(), Is.InstanceOf<NoContentResult>());
        }
    }
}
