using ExamTest.Application.Services.Auth;
using ExamTest.Domain.Entities.Auth;
using ExamTest.Domain.Interfaces.ForRepos;
using Moq;
using NUnit.Framework;

namespace ExamTest.Tests.Application.Services.Auth
{
    [TestFixture]
    public class SellerServiceTests
    {
        private Mock<IRepository<Seller>> _repositoryMock = null!;
        private SellerService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRepository<Seller>>();
            _sut = new SellerService(_repositoryMock.Object);
        }

        [Test]
        public async Task GetAllAsync_DelegatesToRepository()
        {
            var sellers = new List<Seller> { new() { Id = 1, Email = "a@test.com" } };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sellers);

            var result = await _sut.GetAllAsync();

            Assert.That(result, Is.SameAs(sellers));
        }

        [Test]
        public async Task GetByIdAsync_DelegatesToRepository()
        {
            var seller = new Seller { Id = 1, Email = "a@test.com" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(seller);

            var result = await _sut.GetByIdAsync(1);

            Assert.That(result, Is.SameAs(seller));
        }

        [Test]
        public async Task GetByEmail_IsCaseInsensitiveAndTrimsInput()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Seller>
            {
                new() { Id = 1, Email = "seller@example.com" }
            });

            var result = await _sut.GetByEmail("  SELLER@EXAMPLE.COM  ");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetByEmail_WhenNoMatch_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Seller>
            {
                new() { Id = 1, Email = "someone-else@example.com" }
            });

            var result = await _sut.GetByEmail("nobody@example.com");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBySellersIdAsync_ReturnsMatchingSellersOnly()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Seller>
            {
                new() { Id = 1, Email = "a@test.com" },
                new() { Id = 2, Email = "b@test.com" },
                new() { Id = 1, Email = "c@test.com" }
            });

            var result = await _sut.GetBySellersIdAsync(1);

            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task CreateAsync_WhenEmailNotTaken_AddsSellerAndReturnsIt()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Seller>());
            var newSeller = new Seller { Email = "new@test.com" };
            _repositoryMock.Setup(r => r.AddAsync(newSeller)).ReturnsAsync(newSeller);

            var result = await _sut.CreateAsync(newSeller);

            Assert.That(result, Is.SameAs(newSeller));
            _repositoryMock.Verify(r => r.AddAsync(newSeller), Times.Once);
        }

        [Test]
        public async Task CreateAsync_WhenEmailAlreadyTaken_ReturnsNullAndDoesNotAdd()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Seller>
            {
                new() { Id = 1, Email = "taken@test.com" }
            });

            var result = await _sut.CreateAsync(new Seller { Email = "TAKEN@test.com" });

            Assert.That(result, Is.Null);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Seller>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_DelegatesToRepository()
        {
            var seller = new Seller { Email = "a@test.com" };

            await _sut.UpdateAsync(1, seller);

            _repositoryMock.Verify(r => r.UpdateAsync(1, seller), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            await _sut.DeleteAsync(1);

            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}
