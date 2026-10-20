using System.Security.Claims;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class ReviewsControllerTests
    {
        private Mock<IReviewService> _service = null!;
        private ReviewsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IReviewService>();
            _sut = new ReviewsController(_service.Object).WithUser(6, "Customer");
        }

        [Test]
        public async Task GetByProduct_ReturnsTheReviews()
        {
            IReadOnlyList<ReviewDto> reviews = new List<ReviewDto> { new() { Id = 1 } };
            _service.Setup(s => s.GetByProductIdAsync(3)).ReturnsAsync(reviews);

            var result = await _sut.GetByProduct(3);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(reviews));
        }

        [Test]
        public async Task GetById_WhenFound_ReturnsIt()
        {
            var review = new ReviewDto { Id = 1 };
            _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(review);

            var result = await _sut.GetById(1);

            Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(review));
        }

        [Test]
        public async Task GetById_WhenMissing_Returns404()
        {
            _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((ReviewDto?)null);

            var result = await _sut.GetById(1);

            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Create_SavesTheReviewForTheTokenUserAndReturns201()
        {
            var dto = new CreateReviewDto { ProductId = 2, Rating = 5, Comment = "great" };
            _service.Setup(s => s.CreateAsync(dto, 6)).ReturnsAsync(new ReviewDto { Id = 44 });

            var result = await _sut.Create(dto);

            var created = (CreatedAtActionResult)result.Result!;
            Assert.That(created.ActionName, Is.EqualTo(nameof(ReviewsController.GetById)));
            Assert.That(created.RouteValues!["id"], Is.EqualTo(44));
        }

        [Test]
        public async Task Create_WithoutAUserId_ReturnsUnauthorized()
        {
            var sut = new ReviewsController(_service.Object).WithUser(null);

            var result = await sut.Create(new CreateReviewDto());

            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
            _service.Verify(s => s.CreateAsync(It.IsAny<CreateReviewDto>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task Create_AcceptsTheSubClaimAsAnId()
        {
            var sut = new ReviewsController(_service.Object).WithClaims(new Claim("sub", "17"));
            var dto = new CreateReviewDto();
            _service.Setup(s => s.CreateAsync(dto, 17)).ReturnsAsync(new ReviewDto { Id = 1 });

            var result = await sut.Create(dto);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task Delete_ByTheAuthor_Returns204()
        {
            var result = await _sut.Delete(9);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _service.Verify(s => s.DeleteAsync(9, 6, false), Times.Once);
        }

        [TestCase("Admin")]
        [TestCase("Moderator")]
        public async Task Delete_ByStaff_IsFlaggedAsAdmin(string role)
        {
            var sut = new ReviewsController(_service.Object).WithUser(6, role);

            await sut.Delete(9);

            _service.Verify(s => s.DeleteAsync(9, 6, true), Times.Once);
        }

        [Test]
        public async Task Delete_WhenReviewMissing_Returns404()
        {
            _service.Setup(s => s.DeleteAsync(9, 6, false)).ThrowsAsync(new KeyNotFoundException("Review with id 9 not found"));

            var result = await _sut.Delete(9);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Delete_WhenNotTheAuthor_ReturnsForbid()
        {
            _service.Setup(s => s.DeleteAsync(9, 6, false)).ThrowsAsync(new UnauthorizedAccessException());

            Assert.That(await _sut.Delete(9), Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task Delete_WithoutAUserId_ReturnsUnauthorized()
        {
            var sut = new ReviewsController(_service.Object).WithUser(null);

            Assert.That(await sut.Delete(9), Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
}
