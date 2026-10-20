using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Mappings.Shop;
using IPAY.Application.Services.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Shop
{
    [TestFixture]
    public class ReviewServiceTests
    {
        private Mock<IReviewRepository> _reviews = null!;
        private Mock<IProductRepo> _products = null!;
        private ReviewService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<ReviewMapping>()).CreateMapper();
            _reviews = new Mock<IReviewRepository>();
            _products = new Mock<IProductRepo>();
            _sut = new ReviewService(_reviews.Object, _products.Object, mapper);
        }

        [Test]
        public void CreateAsync_WhenProductDoesNotExist_Throws()
        {
            _products.Setup(p => p.GetProductByIdAsync(9)).ReturnsAsync((Product?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.CreateAsync(new CreateReviewDto { ProductId = 9, Rating = 5 }, userId: 1));
            _reviews.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_SavesTheReviewForTheCallerAndReturnsIt()
        {
            _products.Setup(p => p.GetProductByIdAsync(9)).ReturnsAsync(new Product { Id = 9 });
            Review? saved = null;
            _reviews.Setup(r => r.AddAsync(It.IsAny<Review>()))
                .Callback<Review>(r => saved = r)
                .ReturnsAsync((Review r) => { r.Id = 77; return r; });

            var result = await _sut.CreateAsync(new CreateReviewDto { ProductId = 9, Rating = 4, Comment = "nice" }, userId: 3);

            Assert.That(saved!.UserId, Is.EqualTo(3));
            Assert.That(saved.ProductId, Is.EqualTo(9));
            Assert.That(saved.Rating, Is.EqualTo(4));
            Assert.That(saved.Comment, Is.EqualTo("nice"));
            Assert.That(result.Id, Is.EqualTo(77));
            Assert.That(result.Comment, Is.EqualTo("nice"));
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_ReturnsDto()
        {
            _reviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Review { Id = 1, Rating = 3 });

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result!.Rating, Is.EqualTo(3));
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            _reviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Review?)null);

            Assert.That(await _sut.GetByIdAsync(1), Is.Null);
        }

        [Test]
        public async Task GetByProductIdAsync_MapsAllReviews()
        {
            _reviews.Setup(r => r.GetByProductIdAsync(4)).ReturnsAsync(new List<Review>
            {
                new() { Id = 1, ProductId = 4 },
                new() { Id = 2, ProductId = 4 }
            });

            var result = await _sut.GetByProductIdAsync(4);

            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public void DeleteAsync_WhenReviewMissing_Throws()
        {
            _reviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Review?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(1, userId: 5));
        }

        [Test]
        public void DeleteAsync_WhenCallerIsNotTheAuthor_ThrowsAndKeepsTheReview()
        {
            _reviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Review { Id = 1, UserId = 2 });

            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(1, userId: 5));
            _reviews.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task DeleteAsync_AuthorCanDeleteTheirOwnReview()
        {
            _reviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Review { Id = 1, UserId = 5 });

            await _sut.DeleteAsync(1, userId: 5);

            _reviews.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_AdminCanDeleteSomeoneElsesReview()
        {
            _reviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Review { Id = 1, UserId = 2 });

            await _sut.DeleteAsync(1, userId: 5, isAdmin: true);

            _reviews.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}
