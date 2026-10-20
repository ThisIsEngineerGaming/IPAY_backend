using IPAY.Application.Services.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Shop
{
    [TestFixture]
    public class ProductRatingServiceTests
    {
        private Mock<IProductRatingRepo> _repo = null!;
        private ProductRatingService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repo = new Mock<IProductRatingRepo>();
            _sut = new ProductRatingService(_repo.Object);
        }

        [TestCase(0)]
        [TestCase(6)]
        [TestCase(-3)]
        [TestCase(100)]
        public void RateAsync_WithValueOutsideOneToFive_ThrowsBeforeTouchingTheRepo(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.RateAsync(1, "u1", value));
            _repo.Verify(r => r.RateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public async Task RateAsync_WithValidValue_ReturnsTheRepositoryOutcome(int value)
        {
            var outcome = new RateOutcome(RateResult.Rated, 4.5, 2);
            _repo.Setup(r => r.RateAsync(7, "u1", value)).ReturnsAsync(outcome);

            var result = await _sut.RateAsync(7, "u1", value);

            Assert.That(result, Is.SameAs(outcome));
        }

        [Test]
        public async Task GetMineAsync_DelegatesToRepository()
        {
            IReadOnlyDictionary<int, int> mine = new Dictionary<int, int> { [7] = 4 };
            _repo.Setup(r => r.GetByUserAsync("u1")).ReturnsAsync(mine);

            var result = await _sut.GetMineAsync("u1");

            Assert.That(result, Is.SameAs(mine));
        }
    }
}
