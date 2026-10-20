using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Auth
{
    [TestFixture]
    public class SellerRequestServiceTests
    {
        private Mock<IRepository<SellerRequest>> _requests = null!;
        private Mock<IUser<Customer>> _customers = null!;
        private SellerRequestService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _requests = new Mock<IRepository<SellerRequest>>();
            _requests.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SellerRequest>());
            _requests.Setup(r => r.AddAsync(It.IsAny<SellerRequest>()))
                .ReturnsAsync((SellerRequest r) => { r.Id = 100; return r; });

            _customers = new Mock<IUser<Customer>>();
            _sut = new SellerRequestService(_requests.Object, _customers.Object);
        }

        private void GivenCustomer(int id = 5, UserRole role = UserRole.Customer, bool banned = false) =>
            _customers.Setup(c => c.GetByIdAsync(id)).ReturnsAsync(new Customer
            {
                Id = id,
                Name = "Alice",
                Email = "alice@test.com",
                Role = role,
                IsBanned = banned
            });

        // ---------- CreateAsync ----------

        [Test]
        public async Task CreateAsync_WithTooLongMessage_IsRejectedBeforeLookingUpTheUser()
        {
            var result = await _sut.CreateAsync(5, new string('x', 501));

            Assert.That(result.Request, Is.Null);
            Assert.That(result.Error, Does.Contain("500"));
            _customers.Verify(c => c.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_MessageOfExactlyMaxLength_IsAccepted()
        {
            GivenCustomer();

            var result = await _sut.CreateAsync(5, new string('x', 500));

            Assert.That(result.Error, Is.Null);
            Assert.That(result.Request, Is.Not.Null);
        }

        [Test]
        public async Task CreateAsync_WhenUserDoesNotExist_ReturnsNotFound()
        {
            _customers.Setup(c => c.GetByIdAsync(5)).ReturnsAsync((Customer?)null);

            var result = await _sut.CreateAsync(5, "hi");

            Assert.That(result.NotFound, Is.True);
            Assert.That(result.Request, Is.Null);
        }

        [Test]
        public async Task CreateAsync_WhenBanned_IsRejected()
        {
            GivenCustomer(banned: true);

            var result = await _sut.CreateAsync(5, "hi");

            Assert.That(result.Error, Does.Contain("Banned"));
            _requests.Verify(r => r.AddAsync(It.IsAny<SellerRequest>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenAlreadySeller_IsRejected()
        {
            GivenCustomer(role: UserRole.Seller);

            var result = await _sut.CreateAsync(5, "hi");

            Assert.That(result.Error, Does.Contain("already a seller"));
        }

        [TestCase(UserRole.Admin)]
        [TestCase(UserRole.Moderator)]
        [TestCase(UserRole.Guest)]
        public async Task CreateAsync_ForNonCustomerRoles_IsRejected(UserRole role)
        {
            GivenCustomer(role: role);

            var result = await _sut.CreateAsync(5, "hi");

            Assert.That(result.Error, Does.Contain("Only customers"));
            _requests.Verify(r => r.AddAsync(It.IsAny<SellerRequest>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenAPendingRequestExists_IsRejected()
        {
            GivenCustomer();
            _requests.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SellerRequest>
            {
                new() { Id = 1, UserId = 5, Status = SellerRequestStatus.Pending }
            });

            var result = await _sut.CreateAsync(5, "hi");

            Assert.That(result.Error, Does.Contain("already have a request"));
            _requests.Verify(r => r.AddAsync(It.IsAny<SellerRequest>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_IgnoresPendingRequestsOfOtherUsersAndOldReviewedOnes()
        {
            GivenCustomer();
            _requests.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SellerRequest>
            {
                new() { Id = 1, UserId = 99, Status = SellerRequestStatus.Pending },
                new() { Id = 2, UserId = 5, Status = SellerRequestStatus.Rejected }
            });

            var result = await _sut.CreateAsync(5, "please");

            Assert.That(result.Error, Is.Null);
            Assert.That(result.Request!.Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task CreateAsync_SavesTrimmedMessageAndUserDetails()
        {
            GivenCustomer();
            SellerRequest? saved = null;
            _requests.Setup(r => r.AddAsync(It.IsAny<SellerRequest>()))
                .Callback<SellerRequest>(r => saved = r)
                .ReturnsAsync((SellerRequest r) => { r.Id = 100; return r; });

            var result = await _sut.CreateAsync(5, "   I sell books   ");

            Assert.That(saved!.Message, Is.EqualTo("I sell books"));
            Assert.That(saved.UserId, Is.EqualTo(5));
            Assert.That(saved.UserName, Is.EqualTo("Alice"));
            Assert.That(saved.Email, Is.EqualTo("alice@test.com"));
            Assert.That(saved.Status, Is.EqualTo(SellerRequestStatus.Pending));
            Assert.That(result.Request!.Id, Is.EqualTo(100));
        }

        [Test]
        public async Task CreateAsync_WithNullMessage_StoresAnEmptyOne()
        {
            GivenCustomer();
            SellerRequest? saved = null;
            _requests.Setup(r => r.AddAsync(It.IsAny<SellerRequest>()))
                .Callback<SellerRequest>(r => saved = r)
                .ReturnsAsync((SellerRequest r) => r);

            await _sut.CreateAsync(5, null);

            Assert.That(saved!.Message, Is.EqualTo(string.Empty));
        }

        // ---------- GetLatestForUserAsync / GetAllAsync ----------

        [Test]
        public async Task GetLatestForUserAsync_ReturnsTheNewestRequestOfThatUser()
        {
            _requests.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SellerRequest>
            {
                new() { Id = 1, UserId = 5, CreatedAt = new DateTime(2026, 1, 1) },
                new() { Id = 2, UserId = 5, CreatedAt = new DateTime(2026, 3, 1) },
                new() { Id = 3, UserId = 6, CreatedAt = new DateTime(2026, 5, 1) }
            });

            var result = await _sut.GetLatestForUserAsync(5);

            Assert.That(result!.Id, Is.EqualTo(2));
        }

        [Test]
        public async Task GetLatestForUserAsync_WhenUserNeverAsked_ReturnsNull()
        {
            Assert.That(await _sut.GetLatestForUserAsync(5), Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ListsPendingFirstThenNewestFirst()
        {
            _requests.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SellerRequest>
            {
                new() { Id = 1, Status = SellerRequestStatus.Approved, CreatedAt = new DateTime(2026, 5, 1) },
                new() { Id = 2, Status = SellerRequestStatus.Pending, CreatedAt = new DateTime(2026, 1, 1) },
                new() { Id = 3, Status = SellerRequestStatus.Pending, CreatedAt = new DateTime(2026, 2, 1) },
                new() { Id = 4, Status = SellerRequestStatus.Rejected, CreatedAt = new DateTime(2026, 6, 1) }
            });

            var result = await _sut.GetAllAsync();

            Assert.That(result.Select(r => r.Id), Is.EqualTo(new[] { 3, 2, 4, 1 }));
        }

        // ---------- ApproveAsync / RejectAsync ----------

        [Test]
        public async Task ApproveAsync_WhenRequestMissing_ReturnsNotFound()
        {
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((SellerRequest?)null);

            var result = await _sut.ApproveAsync(1, "admin");

            Assert.That(result.NotFound, Is.True);
        }

        [TestCase(SellerRequestStatus.Approved)]
        [TestCase(SellerRequestStatus.Rejected)]
        public async Task ApproveAsync_WhenAlreadyReviewed_IsRejected(SellerRequestStatus status)
        {
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SellerRequest { Id = 1, UserId = 5, Status = status });

            var result = await _sut.ApproveAsync(1, "admin");

            Assert.That(result.Error, Does.Contain("already been reviewed"));
            _requests.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<SellerRequest>()), Times.Never);
        }

        [Test]
        public async Task ApproveAsync_WhenUserWasDeleted_IsRejected()
        {
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SellerRequest { Id = 1, UserId = 5 });
            _customers.Setup(c => c.GetByIdAsync(5)).ReturnsAsync((Customer?)null);

            var result = await _sut.ApproveAsync(1, "admin");

            Assert.That(result.Error, Does.Contain("no longer exists"));
        }

        [Test]
        public async Task ApproveAsync_WhenUserIsBanned_IsRejected()
        {
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SellerRequest { Id = 1, UserId = 5 });
            GivenCustomer(banned: true);

            var result = await _sut.ApproveAsync(1, "admin");

            Assert.That(result.Error, Does.Contain("banned"));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [TestCase(UserRole.Admin)]
        [TestCase(UserRole.Moderator)]
        public async Task ApproveAsync_WhenUserIsStaff_IsRejected(UserRole role)
        {
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SellerRequest { Id = 1, UserId = 5 });
            GivenCustomer(role: role);

            var result = await _sut.ApproveAsync(1, "admin");

            Assert.That(result.Error, Does.Contain("staff"));
        }

        [Test]
        public async Task ApproveAsync_PromotesTheCustomerAndRecordsTheReview()
        {
            var request = new SellerRequest { Id = 1, UserId = 5 };
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);
            GivenCustomer();

            var result = await _sut.ApproveAsync(1, "boss");

            Assert.That(result.Error, Is.Null);
            Assert.That(result.Request!.Status, Is.EqualTo("Approved"));
            Assert.That(request.ReviewedBy, Is.EqualTo("boss"));
            Assert.That(request.ReviewedAt, Is.Not.Null);
            _customers.Verify(c => c.UpdateAsync(5, It.Is<Customer>(u => u.Role == UserRole.Seller)), Times.Once);
            _requests.Verify(r => r.UpdateAsync(1, request), Times.Once);
        }

        [Test]
        public async Task ApproveAsync_WhenUserIsAlreadySeller_JustClosesTheRequest()
        {
            var request = new SellerRequest { Id = 1, UserId = 5 };
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);
            GivenCustomer(role: UserRole.Seller);

            var result = await _sut.ApproveAsync(1, "boss");

            Assert.That(result.Request!.Status, Is.EqualTo("Approved"));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task RejectAsync_ClosesTheRequestWithoutTouchingTheUser()
        {
            var request = new SellerRequest { Id = 1, UserId = 5 };
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);

            var result = await _sut.RejectAsync(1, "boss");

            Assert.That(result.Request!.Status, Is.EqualTo("Rejected"));
            Assert.That(request.ReviewedBy, Is.EqualTo("boss"));
            _customers.Verify(c => c.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task RejectAsync_WhenRequestMissing_ReturnsNotFound()
        {
            _requests.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((SellerRequest?)null);

            var result = await _sut.RejectAsync(1, "boss");

            Assert.That(result.NotFound, Is.True);
        }
    }
}
