using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Mapping;
using IPAY.Application.Services.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.UnitTests.Support;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Shop
{
    [TestFixture]
    public class CartItemServiceTests
    {
        private CartItemService _sut = null!;
        private InMemoryProductRepo _products = null!;
        private int _productId;

        [SetUp]
        public async Task SetUp()
        {
            var items = new InMemoryRepository<CartItem>(i => i.Id, (i, id) => i.Id = id);
            var carts = new InMemoryRepository<Cart>(c => c.Id, (c, id) => c.Id = id);
            _products = new InMemoryProductRepo();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<CartMapping>()).CreateMapper();
            _sut = new CartItemService(items, carts, _products, mapper);

            _productId = (await _products.AddProductAsync(new Product { Name = "Lamp", Price = 20, DiscountedPrice = 0 })).Id;
        }

        [Test]
        public async Task AddAsync_ReturnsItemWithProductDetailsAndPrice()
        {
            var item = await _sut.AddAsync(1, new CreateCartItemDto { ProductId = _productId, Quantity = 2 });

            Assert.That(item.ProductName, Is.EqualTo("Lamp"));
            Assert.That(item.Price, Is.EqualTo(20));
            Assert.That(item.TotalPrice, Is.EqualTo(40));
        }

        [Test]
        public async Task AddAsync_SameProductAgain_IncreasesQuantity()
        {
            await _sut.AddAsync(1, new CreateCartItemDto { ProductId = _productId, Quantity = 2 });

            var item = await _sut.AddAsync(1, new CreateCartItemDto { ProductId = _productId, Quantity = 3 });

            Assert.That(item.Quantity, Is.EqualTo(5));
        }

        [Test]
        public async Task GetByIdAsync_OwnerSeesTheItem_OtherUserGetsNull()
        {
            var item = await _sut.AddAsync(1, new CreateCartItemDto { ProductId = _productId, Quantity = 1 });

            Assert.That(await _sut.GetByIdAsync(1, item.Id), Is.Not.Null);
            Assert.That(await _sut.GetByIdAsync(2, item.Id), Is.Null);
        }

        [Test]
        public async Task UpdateAsync_ChangesQuantityForOwner_ButNotForAnotherUser()
        {
            var item = await _sut.AddAsync(1, new CreateCartItemDto { ProductId = _productId, Quantity = 1 });

            Assert.That(await _sut.UpdateAsync(2, item.Id, new CreateCartItemDto { Quantity = 9 }), Is.Null);
            var updated = await _sut.UpdateAsync(1, item.Id, new CreateCartItemDto { Quantity = 6 });

            Assert.That(updated!.Quantity, Is.EqualTo(6));
        }

        [Test]
        public async Task DeleteAsync_OnlyTheOwnerCanDelete()
        {
            var item = await _sut.AddAsync(1, new CreateCartItemDto { ProductId = _productId, Quantity = 1 });

            Assert.That(await _sut.DeleteAsync(2, item.Id), Is.False);
            Assert.That(await _sut.DeleteAsync(1, item.Id), Is.True);
        }
    }
}
