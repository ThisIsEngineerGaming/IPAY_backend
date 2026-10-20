using System.Security.Claims;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class SellerRequestsControllerTests
    {
        private Mock<ISellerRequestService> _service = null!;
        private SellerRequestsController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ISellerRequestService>();
            _sut = new SellerRequestsController(_service.Object).WithClaims(
                new Claim(ClaimTypes.NameIdentifier, "4"),
                new Claim(ClaimTypes.Email, "boss@test.com"));
        }

        // ---------- Create ----------

        [Test]
        public async Task Create_UsesTheTokenUserAndTheMessage()
        {
            var dto = new SellerRequestDto { Id = 1 };
            _service.Setup(s => s.CreateAsync(4, "hello")).ReturnsAsync(new SellerRequestResult(dto));

            var result = await _sut.Create(new CreateSellerRequestDto { Message = "hello" });

            Assert.That(((OkObjectResult)result).Value, Is.SameAs(dto));
        }

        [Test]
        public async Task Create_WithoutBody_StillWorks()
        {
            _service.Setup(s => s.CreateAsync(4, null)).ReturnsAsync(new SellerRequestResult(new SellerRequestDto()));

            Assert.That(await _sut.Create(null), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Create_WithoutAUserId_ReturnsUnauthorized()
        {
            var sut = new SellerRequestsController(_service.Object).WithUser(null);

            Assert.That(await sut.Create(new CreateSellerRequestDto()), Is.InstanceOf<UnauthorizedResult>());
            Assert.That(await sut.GetMine(), Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Create_WhenServiceSaysNotFound_Returns404()
        {
            _service.Setup(s => s.CreateAsync(4, It.IsAny<string?>()))
                .ReturnsAsync(new SellerRequestResult(null, "User not found.", NotFound: true));

            var result = await _sut.Create(new CreateSellerRequestDto());

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Create_WhenServiceReportsAProblem_Returns400WithTheMessage()
        {
            _service.Setup(s => s.CreateAsync(4, It.IsAny<string?>()))
                .ReturnsAsync(new SellerRequestResult(null, "You are already a seller."));

            var result = await _sut.Create(new CreateSellerRequestDto());

            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("You are already a seller."));
        }

        // ---------- GetMine ----------

        [Test]
        public async Task GetMine_WhenTheyNeverAsked_Returns204()
        {
            _service.Setup(s => s.GetLatestForUserAsync(4)).ReturnsAsync((SellerRequestDto?)null);

            Assert.That(await _sut.GetMine(), Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetMine_ReturnsTheLatestRequest()
        {
            var dto = new SellerRequestDto { Id = 9 };
            _service.Setup(s => s.GetLatestForUserAsync(4)).ReturnsAsync(dto);

            Assert.That(((OkObjectResult)await _sut.GetMine()).Value, Is.SameAs(dto));
        }

        // ---------- staff ----------

        [Test]
        public async Task GetAll_ReturnsEveryRequest()
        {
            IReadOnlyList<SellerRequestDto> all = new List<SellerRequestDto> { new() { Id = 1 } };
            _service.Setup(s => s.GetAllAsync()).ReturnsAsync(all);

            Assert.That(((OkObjectResult)await _sut.GetAll()).Value, Is.SameAs(all));
        }

        [Test]
        public async Task Approve_RecordsTheReviewersEmail()
        {
            _service.Setup(s => s.ApproveAsync(3, "boss@test.com")).ReturnsAsync(new SellerRequestResult(new SellerRequestDto { Id = 3 }));

            var result = await _sut.Approve(3);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _service.Verify(s => s.ApproveAsync(3, "boss@test.com"), Times.Once);
        }

        [Test]
        public async Task Reject_RecordsTheReviewersEmail()
        {
            _service.Setup(s => s.RejectAsync(3, "boss@test.com")).ReturnsAsync(new SellerRequestResult(new SellerRequestDto { Id = 3 }));

            Assert.That(await _sut.Reject(3), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Approve_WithoutAnEmailClaim_FallsBackToTheUserId()
        {
            var sut = new SellerRequestsController(_service.Object).WithUser(4, "Admin");
            _service.Setup(s => s.ApproveAsync(3, "4")).ReturnsAsync(new SellerRequestResult(new SellerRequestDto()));

            await sut.Approve(3);

            _service.Verify(s => s.ApproveAsync(3, "4"), Times.Once);
        }

        [Test]
        public async Task Approve_WithoutAnyIdentity_FallsBackToStaff()
        {
            var sut = new SellerRequestsController(_service.Object).WithClaims();
            _service.Setup(s => s.ApproveAsync(3, "staff")).ReturnsAsync(new SellerRequestResult(new SellerRequestDto()));

            await sut.Approve(3);

            _service.Verify(s => s.ApproveAsync(3, "staff"), Times.Once);
        }

        [Test]
        public async Task Approve_WhenRequestMissing_Returns404()
        {
            _service.Setup(s => s.ApproveAsync(3, It.IsAny<string>()))
                .ReturnsAsync(new SellerRequestResult(null, "Request not found.", NotFound: true));

            Assert.That(await _sut.Approve(3), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Reject_WhenAlreadyReviewed_Returns400()
        {
            _service.Setup(s => s.RejectAsync(3, It.IsAny<string>()))
                .ReturnsAsync(new SellerRequestResult(null, "This request has already been reviewed."));

            Assert.That(await _sut.Reject(3), Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}
