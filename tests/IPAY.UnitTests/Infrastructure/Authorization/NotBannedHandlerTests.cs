using System.Security.Claims;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Authorization
{
    [TestFixture]
    public class NotBannedHandlerTests
    {
        private Mock<IUser<Customer>> _customersMock = null!;
        private NotBannedHandler _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _customersMock = new Mock<IUser<Customer>>();
            _sut = new NotBannedHandler(_customersMock.Object);
        }

        private static ClaimsPrincipal User(int? id)
        {
            var claims = new List<Claim>();
            if (id is not null) claims.Add(new Claim(ClaimTypes.NameIdentifier, id.Value.ToString()));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }

        private async Task<bool> Evaluate(ClaimsPrincipal user)
        {
            var requirement = new NotBannedRequirement();
            var context = new AuthorizationHandlerContext(new IAuthorizationRequirement[] { requirement }, user, null);
            await _sut.HandleAsync(context);
            return context.HasSucceeded;
        }

        [Test]
        public async Task ActiveUser_Succeeds()
        {
            _customersMock.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1, IsBanned = false });

            Assert.That(await Evaluate(User(1)), Is.True);
        }

        [Test]
        public async Task BannedUser_IsDenied()
        {
            _customersMock.Setup(c => c.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1, IsBanned = true });

            Assert.That(await Evaluate(User(1)), Is.False);
        }

        [Test]
        public async Task UserWhoNoLongerExists_IsDenied()
        {
            _customersMock.Setup(c => c.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);

            Assert.That(await Evaluate(User(77)), Is.False);
        }

        [Test]
        public async Task TokenWithoutUserId_IsDeniedWithoutLookingAnyoneUp()
        {
            Assert.That(await Evaluate(User(null)), Is.False);
            _customersMock.Verify(c => c.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
