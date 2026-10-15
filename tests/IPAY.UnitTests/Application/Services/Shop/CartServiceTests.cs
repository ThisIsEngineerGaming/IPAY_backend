using AutoMapper;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Mapping;
using IPAY.Application.Services.Shop.IPAY.Application.Services.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.UnitTests.Support;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Services.Shop
{
    [TestFixture]
    public class CartServiceTests
    {
        private InMemoryRepository<Cart> _carts = null!;
        private InMemoryProductRepo _products = null!;
        private CartService _sut = null!;
        private int _phoneId;

        [SetUp]
        public async Task SetUp()
        {
            _carts = new InMemoryRepository<Cart>(c => c.Id, (c, id) => c.Id = id);
            _products = new InMemoryProductRepo();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<CartMapping>()).CreateMapper();
            _sut = new CartService(_carts, _products, mapper);

            // list price 100, discounted 75 -> customers pay 75
            _phoneId = (await _products.AddProductAsync(new Product
            {
                Name = "Phone", Price = 100, DiscountedPrice = 75, ImageUrl = "https://img/phone.png"
            })).Id;
        }

        [Test]
        public async Task GetByUserIdAsync_WhenUserHasNoCart_ReturnsNull()
        {
            Assert.That(await _sut.GetByUserIdAsync(1), Is.Null);
        }

        [Test]
        public async Task AddItemAsync_CreatesCartAndFillsNameImageAndFinalPriceFromProduct()
        {
            var cart = await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 2 });

            Assert.That(cart.Items, Has.Count.EqualTo(1));
            var item = cart.Items[0];
            Assert.That(item.ProductName, Is.EqualTo("Phone"));
            Assert.That(item.ImageUrl, Is.EqualTo("https://img/phone.png"));
            Assert.That(item.Price, Is.EqualTo(75));
            Assert.That(cart.TotalPrice, Is.EqualTo(150));
            Assert.That(cart.TotalItemsCount, Is.EqualTo(2));
        }

        [Test]
        public async Task AddItemAsync_SameProductTwice_MergesQuantityInsteadOfDuplicatingTheLine()
        {
            await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });

            var cart = await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 3 });

            Assert.That(cart.Items, Has.Count.EqualTo(1));
            Assert.That(cart.Items[0].Quantity, Is.EqualTo(4));
        }

        [Test]
        public async Task AddItemAsync_IsPersisted_SoALaterReadSeesIt()
        {
            await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 2 });

            var reloaded = await _sut.GetByUserIdAsync(1);

            Assert.That(reloaded!.Items.Single().Quantity, Is.EqualTo(2));
        }

        [Test]
        public async Task Carts_AreSeparatePerUser()
        {
            await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });
            await _sut.AddItemAsync(2, new CreateCartItemDto { ProductId = _phoneId, Quantity = 5 });

            Assert.That((await _sut.GetByUserIdAsync(1))!.TotalItemsCount, Is.EqualTo(1));
            Assert.That((await _sut.GetByUserIdAsync(2))!.TotalItemsCount, Is.EqualTo(5));
        }

        [Test]
        public async Task UpdateItemQuantityAsync_SetsNewQuantity()
        {
            var cart = await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });
            var itemId = cart.Items[0].Id;

            var updated = await _sut.UpdateItemQuantityAsync(1, itemId, new CreateCartItemDto { ProductId = _phoneId, Quantity = 7 });

            Assert.That(updated!.Items.Single().Quantity, Is.EqualTo(7));
        }

        [Test]
        public async Task UpdateItemQuantityAsync_WithZeroQuantity_RemovesTheLine()
        {
            var cart = await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });

            var updated = await _sut.UpdateItemQuantityAsync(1, cart.Items[0].Id, new CreateCartItemDto { ProductId = _phoneId, Quantity = 0 });

            Assert.That(updated!.Items, Is.Empty);
        }

        [Test]
        public async Task UpdateItemQuantityAsync_ForUnknownItem_ReturnsNull()
        {
            await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });

            Assert.That(await _sut.UpdateItemQuantityAsync(1, 999, new CreateCartItemDto { Quantity = 2 }), Is.Null);
        }

        [Test]
        public async Task RemoveItemAsync_ByAUserWithoutThatItem_ReturnsNullAndLeavesTheOwnersCartAlone()
        {
            var cart = await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });

            // user 2 has no cart, so line id from user 1's cart means nothing to them
            var result = await _sut.RemoveItemAsync(2, cart.Items[0].Id);

            Assert.That(result, Is.Null);
            Assert.That((await _sut.GetByUserIdAsync(1))!.Items, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task RemoveItemAsync_RemovesOnlyThatLine()
        {
            var other = (await _products.AddProductAsync(new Product { Name = "Case", Price = 10 })).Id;
            await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });
            var cart = await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = other, Quantity = 1 });
            var phoneLine = cart.Items.Single(i => i.ProductId == _phoneId);

            var after = await _sut.RemoveItemAsync(1, phoneLine.Id);

            Assert.That(after!.Items.Single().ProductId, Is.EqualTo(other));
        }

        [Test]
        public async Task ClearCartAsync_EmptiesTheCart_AndIsSafeWhenThereIsNone()
        {
            await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 2 });

            await _sut.ClearCartAsync(1);
            await _sut.ClearCartAsync(42); // no cart: must not throw

            Assert.That((await _sut.GetByUserIdAsync(1))!.Items, Is.Empty);
        }

        [Test]
        public async Task GetByUserIdAsync_WhenProductWasDeleted_KeepsTheLineWithoutCrashing()
        {
            await _sut.AddItemAsync(1, new CreateCartItemDto { ProductId = _phoneId, Quantity = 1 });
            await _products.DeleteProductAsync(_phoneId);

            var cart = await _sut.GetByUserIdAsync(1);

            Assert.That(cart!.Items, Has.Count.EqualTo(1));
            Assert.That(cart.Items[0].Price, Is.EqualTo(0));
        }
    }
}
