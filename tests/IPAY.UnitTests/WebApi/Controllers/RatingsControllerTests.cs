using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class RatingsControllerTests
    {
        private Mock<IProductRatingService> _service = null!;
        private RatingsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IProductRatingService>();
            _sut = new RatingsController(_service.Object).WithUser(12, "Customer");
        }

        private static object? Prop(object? value, string name) => value!.GetType().GetProperty(name)!.GetValue(value);

        [Test]
        public async Task GetMine_ReturnsTheRatingsOfTheTokenUser()
        {
            IReadOnlyDictionary<int, int> mine = new Dictionary<int, int> { [3] = 5 };
            _service.Setup(s => s.GetMineAsync("12")).ReturnsAsync(mine);

            var result = await _sut.GetMine();

            Assert.That(((OkObjectResult)result).Value, Is.SameAs(mine));
        }

        [Test]
        public async Task GetMine_WithoutUserId_ReturnsUnauthorized()
        {
            var sut = new RatingsController(_service.Object).WithUser(null);

            Assert.That(await sut.GetMine(), Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Rate_WithoutUserId_ReturnsUnauthorized()
        {
            var sut = new RatingsController(_service.Object).WithUser(null);

            Assert.That(await sut.Rate(1, new RateProductDto { Value = 5 }), Is.InstanceOf<UnauthorizedResult>());
        }

        [TestCase(0)]
        [TestCase(6)]
        [TestCase(-1)]
        public async Task Rate_WithValueOutOfRange_Returns400WithoutCallingTheService(int value)
        {
            var result = await _sut.Rate(1, new RateProductDto { Value = value });

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _service.Verify(s => s.RateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task Rate_WithNoBody_Returns400()
        {
            Assert.That(await _sut.Rate(1, null!), Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Rate_OnSuccess_ReturnsTheNewAverageAndTheCallersStars()
        {
            _service.Setup(s => s.RateAsync(7, "12", 4)).ReturnsAsync(new RateOutcome(RateResult.Rated, 4.2, 10));

            var result = await _sut.Rate(7, new RateProductDto { Value = 4 });

            var body = ((OkObjectResult)result).Value;
            Assert.That(Prop(body, "rating"), Is.EqualTo(4.2));
            Assert.That(Prop(body, "ratingCount"), Is.EqualTo(10));
            Assert.That(Prop(body, "myRating"), Is.EqualTo(4));
        }

        [Test]
        public async Task Rate_WhenProductMissing_Returns404()
        {
            _service.Setup(s => s.RateAsync(7, "12", 4)).ReturnsAsync(new RateOutcome(RateResult.ProductNotFound));

            Assert.That(await _sut.Rate(7, new RateProductDto { Value = 4 }), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Rate_WhenAlreadyRated_Returns409()
        {
            _service.Setup(s => s.RateAsync(7, "12", 4)).ReturnsAsync(new RateOutcome(RateResult.AlreadyRated));

            Assert.That(await _sut.Rate(7, new RateProductDto { Value = 4 }), Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task Rate_OnYourOwnProduct_Returns403()
        {
            _service.Setup(s => s.RateAsync(7, "12", 4)).ReturnsAsync(new RateOutcome(RateResult.OwnProduct));

            var result = await _sut.Rate(7, new RateProductDto { Value = 4 });

            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(403));
        }
    }
}
