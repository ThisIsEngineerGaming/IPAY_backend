using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.WebApi.Controllers
{
    [TestFixture]
    public class AdminControllerTests
    {
        private Mock<IUser<Customer>> _customers = null!;
        private Mock<IGuestService> _guests = null!;
        private Mock<IPasswordHashingService> _hasher = null!;
        private AdminController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _customers = new Mock<IUser<Customer>>();
            _guests = new Mock<IGuestService>();
            _hasher = new Mock<IPasswordHashingService>();
            _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns((string p) => "hash:" + p);
            _sut = new AdminController(_customers.Object, _guests.Object, _hasher.Object);
        }

        private Customer GivenUser(int id = 5, UserRole role = UserRole.Customer, bool banned = false)
        {
            var user = new Customer { Id = id, Name = "Alice", Email = "a@test.com", Role = role, IsBanned = banned };
            _customers.Setup(c => c.GetByIdAsync(id)).ReturnsAsync(user);
            return user;
        }

        private void GivenNoUser(int id = 5) =>
            _customers.Setup(c => c.GetByIdAsync(id)).ReturnsAsync((Customer?)null);

        private static void AssertStatus(IActionResult result, int status) =>
            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(status));

        // ---------- ban / unban by admin ----------

        [Test]
        public async Task BannedByAdmin_BansTheUserAndReturnsTheAdminView()
        {
            var user = GivenUser();

            var result = await _sut.BannedByAdmin(5);

            Assert.That(user.IsBanned, Is.True);
            var dto = (AdminUserDto)((OkObjectResult)result).Value!;
            Assert.That(dto.Id, Is.EqualTo(5));
            Assert.That(dto.IsBanned, Is.True);
            Assert.That(dto.Role, Is.EqualTo("Customer"));
            _customers.Verify(c => c.UpdateAsync(5, user), Times.Once);
        }

        [Test]
        public async Task BannedByAdmin_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.BannedByAdmin(5), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task BannedByAdmin_RefusesToBanAnAdmin()
        {
            var admin = GivenUser(role: UserRole.Admin);

            var result = await _sut.BannedByAdmin(5);

            AssertStatus(result, 403);
            Assert.That(admin.IsBanned, Is.False);
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task UnBannedByAdmin_LiftsTheBan()
        {
            var user = GivenUser(banned: true);

            var result = await _sut.UnBannedByAdmin(5);

            Assert.That(user.IsBanned, Is.False);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UnBannedByAdmin_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.UnBannedByAdmin(5), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UnBannedByAdmin_RefusesToTouchAnAdmin()
        {
            GivenUser(role: UserRole.Admin);

            AssertStatus(await _sut.UnBannedByAdmin(5), 403);
        }

        // ---------- ban / unban by moderator ----------

        [Test]
        public async Task BannedByModerator_CanBanACustomer()
        {
            var user = GivenUser();

            var result = await _sut.BannedByModerator(5);

            Assert.That(user.IsBanned, Is.True);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [TestCase(UserRole.Admin)]
        [TestCase(UserRole.Moderator)]
        public async Task BannedByModerator_CannotBanAdminsOrOtherModerators(UserRole role)
        {
            GivenUser(role: role);

            AssertStatus(await _sut.BannedByModerator(5), 403);
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task BannedByModerator_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.BannedByModerator(5), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UnBannedByModerator_CanUnbanACustomer()
        {
            var user = GivenUser(banned: true);

            await _sut.UnBannedByModerator(5);

            Assert.That(user.IsBanned, Is.False);
        }

        [TestCase(UserRole.Admin)]
        [TestCase(UserRole.Moderator)]
        public async Task UnBannedByModerator_CannotChangeStaff(UserRole role)
        {
            GivenUser(role: role, banned: true);

            AssertStatus(await _sut.UnBannedByModerator(5), 403);
        }

        [Test]
        public async Task UnBannedByModerator_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.UnBannedByModerator(5), Is.InstanceOf<NotFoundObjectResult>());
        }

        // ---------- ChangeRole (admin) ----------

        [Test]
        public async Task ChangeRole_NobodyCanBeMadeAdmin()
        {
            GivenUser();

            var result = await _sut.ChangeRole(5, UserRole.Admin);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task ChangeRole_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.ChangeRole(5, UserRole.Seller), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task ChangeRole_SavesTheNewRole()
        {
            var user = GivenUser();

            var result = await _sut.ChangeRole(5, UserRole.Moderator);

            Assert.That(user.Role, Is.EqualTo(UserRole.Moderator));
            Assert.That(((OkObjectResult)result).Value, Is.SameAs(user));
            _customers.Verify(c => c.UpdateAsync(5, user), Times.Once);
        }

        // ---------- ChangeRoleByModerator ----------

        [Test]
        public async Task ChangeRoleByModerator_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.ChangeRoleByModerator(5, UserRole.Seller), Is.InstanceOf<NotFoundResult>());
        }

        [TestCase(UserRole.Admin)]
        [TestCase(UserRole.Moderator)]
        public async Task ChangeRoleByModerator_CannotTouchStaff(UserRole current)
        {
            var target = GivenUser(role: current);

            var result = await _sut.ChangeRoleByModerator(5, UserRole.Customer);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
            Assert.That(target.Role, Is.EqualTo(current));
        }

        [TestCase(UserRole.Moderator)]
        [TestCase(UserRole.Admin)]
        public async Task ChangeRoleByModerator_CannotCreateStaff(UserRole requested)
        {
            var target = GivenUser();

            var result = await _sut.ChangeRoleByModerator(5, requested);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(target.Role, Is.EqualTo(UserRole.Customer));
        }

        [Test]
        public async Task ChangeRoleByModerator_CanMakeASeller()
        {
            var target = GivenUser();

            var result = await _sut.ChangeRoleByModerator(5, UserRole.Seller);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(target.Role, Is.EqualTo(UserRole.Seller));
            _customers.Verify(c => c.UpdateAsync(5, target), Times.Once);
        }

        // ---------- GetUsers ----------

        [Test]
        public async Task GetUsers_ListsEveryoneOrderedByIdWithoutPasswords()
        {
            _customers.Setup(c => c.GetAllAsync()).ReturnsAsync(new List<Customer>
            {
                new() { Id = 3, Name = "C", Email = "c@test.com", Password = "secret" },
                new() { Id = 1, Name = "A", Email = "a@test.com", Phone = "+123", Role = UserRole.Seller }
            });

            var result = await _sut.GetUsers();

            var list = (List<AdminUserDto>)((OkObjectResult)result).Value!;
            Assert.That(list.Select(u => u.Id), Is.EqualTo(new[] { 1, 3 }));
            Assert.That(list[0].Phone, Is.EqualTo("+123"));
            Assert.That(list[0].Role, Is.EqualTo("Seller"));
        }

        // ---------- promote / demote ----------

        [Test]
        public async Task MakeSeller_PromotesACustomer()
        {
            var user = GivenUser();

            var result = await _sut.MakeSeller(5);

            Assert.That(user.Role, Is.EqualTo(UserRole.Seller));
            Assert.That(((AdminUserDto)((OkObjectResult)result).Value!).Role, Is.EqualTo("Seller"));
        }

        [Test]
        public async Task MakeSeller_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.MakeSeller(5), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task MakeSeller_WhenAlreadySeller_IsABadRequest()
        {
            GivenUser(role: UserRole.Seller);

            Assert.That(await _sut.MakeSeller(5), Is.InstanceOf<BadRequestObjectResult>());
        }

        [TestCase(UserRole.Admin)]
        [TestCase(UserRole.Moderator)]
        public async Task MakeSeller_CannotChangeStaff(UserRole role)
        {
            GivenUser(role: role);

            AssertStatus(await _sut.MakeSeller(5), 403);
        }

        [Test]
        public async Task MakeSeller_RefusesBannedAccounts()
        {
            var user = GivenUser(banned: true);

            var result = await _sut.MakeSeller(5);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(user.Role, Is.EqualTo(UserRole.Customer));
        }

        [Test]
        public async Task MakeModerator_PromotesACustomer()
        {
            var user = GivenUser();

            await _sut.MakeModerator(5);

            Assert.That(user.Role, Is.EqualTo(UserRole.Moderator));
        }

        [Test]
        public async Task MakeModerator_WhenAlreadyModerator_IsABadRequest()
        {
            GivenUser(role: UserRole.Moderator);

            Assert.That(await _sut.MakeModerator(5), Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MakeModerator_CannotChangeAnAdmin()
        {
            GivenUser(role: UserRole.Admin);

            AssertStatus(await _sut.MakeModerator(5), 403);
        }

        [Test]
        public async Task RemoveModerator_TurnsAModeratorIntoACustomer()
        {
            var user = GivenUser(role: UserRole.Moderator);

            var result = await _sut.RemoveModerator(5);

            Assert.That(user.Role, Is.EqualTo(UserRole.Customer));
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RemoveModerator_ForANonModerator_IsABadRequest()
        {
            GivenUser(role: UserRole.Customer);

            var result = await _sut.RemoveModerator(5);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("This user is not a moderator."));
        }

        [Test]
        public async Task RemoveModerator_CannotChangeAnAdmin()
        {
            GivenUser(role: UserRole.Admin);

            AssertStatus(await _sut.RemoveModerator(5), 403);
        }

        [Test]
        public async Task RemoveModerator_WhenUserMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.RemoveModerator(5), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task MakeCustomer_DemotesASeller()
        {
            var user = GivenUser(role: UserRole.Seller);

            await _sut.MakeCustomer(5);

            Assert.That(user.Role, Is.EqualTo(UserRole.Customer));
        }

        [Test]
        public async Task MakeCustomer_CannotDemoteAModerator()
        {
            var user = GivenUser(role: UserRole.Moderator);

            var result = await _sut.MakeCustomer(5);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(user.Role, Is.EqualTo(UserRole.Moderator));
        }

        // ---------- customers CRUD ----------

        [Test]
        public async Task CreateCustomer_HashesThePasswordAndNormalisesTheEmail()
        {
            Customer? saved = null;
            _customers.Setup(c => c.CreateAsync(It.IsAny<Customer>()))
                .Callback<Customer>(c => saved = c)
                .ReturnsAsync((Customer c) => c);

            var result = await _sut.CreateCustomer(new Customer { Email = "  NEW@Test.com ", Password = "Secret1!", Name = "N" });

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(saved!.Password, Is.EqualTo("hash:Secret1!"));
            Assert.That(saved.Email, Is.EqualTo("new@test.com"));
            Assert.That(saved.EmailVerified, Is.True);
        }

        [Test]
        public async Task CreateCustomer_WithoutPassword_DoesNotHash()
        {
            _customers.Setup(c => c.CreateAsync(It.IsAny<Customer>())).ReturnsAsync((Customer c) => c);

            await _sut.CreateCustomer(new Customer { Email = "a@test.com", Password = "" });

            _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task CreateCustomer_KeepsAnExplicitEmailVerifiedFlag()
        {
            Customer? saved = null;
            _customers.Setup(c => c.CreateAsync(It.IsAny<Customer>()))
                .Callback<Customer>(c => saved = c)
                .ReturnsAsync((Customer c) => c);

            await _sut.CreateCustomer(new Customer { Email = "a@test.com", EmailVerified = false });

            Assert.That(saved!.EmailVerified, Is.False);
        }

        [Test]
        public async Task CreateCustomer_NobodyCanBeCreatedAsAdmin()
        {
            var result = await _sut.CreateCustomer(new Customer { Email = "a@test.com", Role = UserRole.Admin });

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _customers.Verify(c => c.CreateAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task CreateCustomer_WhenEmailTaken_IsABadRequest()
        {
            _customers.Setup(c => c.CreateAsync(It.IsAny<Customer>())).ReturnsAsync((Customer?)null);

            Assert.That(await _sut.CreateCustomer(new Customer { Email = "a@test.com" }), Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAllCustomers_ReturnsEveryone()
        {
            var all = new List<Customer> { new() { Id = 1 } };
            _customers.Setup(c => c.GetAllAsync()).ReturnsAsync(all);

            var result = await _sut.GetAllCustomers();

            Assert.That(((OkObjectResult)result).Value, Is.SameAs(all));
        }

        [Test]
        public async Task GetCustomer_WhenFound_ReturnsIt()
        {
            var user = GivenUser();

            Assert.That(((OkObjectResult)await _sut.GetCustomer(5)).Value, Is.SameAs(user));
        }

        [Test]
        public async Task GetCustomer_WhenMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.GetCustomer(5), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateCustomer_WhenMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.UpdateCustomer(5, new Customer()), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateCustomer_OnlyAppliesFieldsThatWereSent()
        {
            var existing = GivenUser();
            existing.Password = "hash:old";

            await _sut.UpdateCustomer(5, new Customer
            {
                Name = "  Bob  ",
                Email = "  BOB@Test.com ",
                Password = "",
                Role = UserRole.Seller,
                IsBanned = true
            });

            Assert.That(existing.Name, Is.EqualTo("Bob"));
            Assert.That(existing.Email, Is.EqualTo("bob@test.com"));
            Assert.That(existing.Password, Is.EqualTo("hash:old"));
            Assert.That(existing.Role, Is.EqualTo(UserRole.Seller));
            Assert.That(existing.IsBanned, Is.True);
            _customers.Verify(c => c.UpdateAsync(5, existing), Times.Once);
        }

        [Test]
        public async Task UpdateCustomer_HashesANewPassword()
        {
            var existing = GivenUser();

            await _sut.UpdateCustomer(5, new Customer { Password = "Fresh1!" });

            Assert.That(existing.Password, Is.EqualTo("hash:Fresh1!"));
        }

        [Test]
        public async Task UpdateCustomer_CannotPromoteSomeoneToAdmin()
        {
            var existing = GivenUser();

            var result = await _sut.UpdateCustomer(5, new Customer { Role = UserRole.Admin });

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(existing.Role, Is.EqualTo(UserRole.Customer));
            _customers.Verify(c => c.UpdateAsync(It.IsAny<int>(), It.IsAny<Customer>()), Times.Never);
        }

        [Test]
        public async Task UpdateCustomer_KeepsEmailVerifiedWhenNoneWasSent()
        {
            var existing = GivenUser();
            existing.EmailVerified = true;

            await _sut.UpdateCustomer(5, new Customer { EmailVerified = null });

            Assert.That(existing.EmailVerified, Is.True);
        }

        [Test]
        public async Task DeleteCustomer_WhenFound_DeletesIt()
        {
            GivenUser();

            var result = await _sut.DeleteCustomer(5);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _customers.Verify(c => c.DeleteAsync(5), Times.Once);
        }

        [Test]
        public async Task DeleteCustomer_WhenMissing_ReturnsNotFound()
        {
            GivenNoUser();

            Assert.That(await _sut.DeleteCustomer(5), Is.InstanceOf<NotFoundObjectResult>());
            _customers.Verify(c => c.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        // ---------- guests CRUD ----------

        [Test]
        public async Task GetAllGuests_ReturnsThem()
        {
            IReadOnlyList<GuestDto> guests = new List<GuestDto> { new() { Id = 1 } };
            _guests.Setup(g => g.GetAllAsync()).ReturnsAsync(guests);

            Assert.That(((OkObjectResult)await _sut.GetAllGuests()).Value, Is.SameAs(guests));
        }

        [Test]
        public async Task GetGuest_WhenFound_ReturnsIt()
        {
            var dto = new GuestDto { Id = 2 };
            _guests.Setup(g => g.GetByIdAsync(2)).ReturnsAsync(dto);

            Assert.That(((OkObjectResult)await _sut.GetGuest(2)).Value, Is.SameAs(dto));
        }

        [Test]
        public async Task GetGuest_WhenMissing_ReturnsNotFound()
        {
            _guests.Setup(g => g.GetByIdAsync(2)).ReturnsAsync((GuestDto?)null);

            Assert.That(await _sut.GetGuest(2), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreateGuest_ReturnsTheCreatedGuest()
        {
            var created = new GuestDto { Id = 3 };
            _guests.Setup(g => g.CreateAsync(It.IsAny<GuestSessionDto>())).ReturnsAsync(created);

            Assert.That(((OkObjectResult)await _sut.CreateGuest(new GuestSessionDto())).Value, Is.SameAs(created));
        }

        [Test]
        public async Task CreateGuest_WhenCreationFails_IsABadRequest()
        {
            _guests.Setup(g => g.CreateAsync(It.IsAny<GuestSessionDto>())).ReturnsAsync((GuestDto?)null);

            Assert.That(await _sut.CreateGuest(new GuestSessionDto()), Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateGuest_WhenFound_ReturnsTheUpdatedGuest()
        {
            var updated = new GuestDto { Id = 3, Name = "New" };
            _guests.Setup(g => g.UpdateAsync(3, It.IsAny<UpdateGuestDto>())).ReturnsAsync(updated);

            Assert.That(((OkObjectResult)await _sut.UpdateGuest(3, new UpdateGuestDto())).Value, Is.SameAs(updated));
        }

        [Test]
        public async Task UpdateGuest_WhenMissing_ReturnsNotFound()
        {
            _guests.Setup(g => g.UpdateAsync(3, It.IsAny<UpdateGuestDto>())).ReturnsAsync((GuestDto?)null);

            Assert.That(await _sut.UpdateGuest(3, new UpdateGuestDto()), Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteGuest_WhenDeleted_ReturnsOk()
        {
            _guests.Setup(g => g.DeleteAsync(3)).ReturnsAsync(true);

            Assert.That(await _sut.DeleteGuest(3), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteGuest_WhenMissing_ReturnsNotFound()
        {
            _guests.Setup(g => g.DeleteAsync(3)).ReturnsAsync(false);

            Assert.That(await _sut.DeleteGuest(3), Is.InstanceOf<NotFoundObjectResult>());
        }
    }
}
