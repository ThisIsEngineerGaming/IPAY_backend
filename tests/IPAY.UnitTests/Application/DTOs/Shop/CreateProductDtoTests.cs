using IPAY.Application.DTOs.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.DTOs.Shop;

[TestFixture]
public sealed class CreateProductDtoTests
{
    [Test]
    public void BaseDto_Defaults_AreInitializedAsExpected()
    {
        var dto = new CreateProductDto();
        Assert.Multiple(() =>
        {
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.ImageUrl, Is.Empty);
            Assert.That(dto.Manufacturer, Is.Empty);
            Assert.That(dto.Price, Is.Zero);
            Assert.That(dto.DiscountedPrice, Is.Zero);
            Assert.That(dto.CategoryId, Is.Zero);
        });
    }

    [Test]
    public void AdminDto_Properties_RoundTripAssignedValues()
    {
        var dto = new CreateAdminProductDto
        {
            Id = 9, Name = "Admin item", Price = 10, DiscountedPrice = 8,
            ImageUrl = "https://example.com/a.png", Manufacturer = "Acme",
            CategoryId = 3, DiscountPercent = 20
        };
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(9));
            Assert.That(dto.Name, Is.EqualTo("Admin item"));
            Assert.That(dto.Price, Is.EqualTo(10));
            Assert.That(dto.DiscountedPrice, Is.EqualTo(8));
            Assert.That(dto.ImageUrl, Is.EqualTo("https://example.com/a.png"));
            Assert.That(dto.Manufacturer, Is.EqualTo("Acme"));
            Assert.That(dto.CategoryId, Is.EqualTo(3));
            Assert.That(dto.DiscountPercent, Is.EqualTo(20));
        });
    }

    [Test]
    public void SellerDto_Properties_RoundTripAssignedValues()
    {
        var dto = new CreateSellerProductDto
        {
            Name = "Seller item", Price = 20, DiscountedPrice = 15,
            ImageUrl = "https://example.com/s.png", Manufacturer = "Seller Co",
            CategoryId = 4, DiscountPercent = 25
        };
        Assert.Multiple(() =>
        {
            Assert.That(dto.Name, Is.EqualTo("Seller item"));
            Assert.That(dto.Price, Is.EqualTo(20));
            Assert.That(dto.DiscountedPrice, Is.EqualTo(15));
            Assert.That(dto.ImageUrl, Is.EqualTo("https://example.com/s.png"));
            Assert.That(dto.Manufacturer, Is.EqualTo("Seller Co"));
            Assert.That(dto.CategoryId, Is.EqualTo(4));
            Assert.That(dto.DiscountPercent, Is.EqualTo(25));
        });
    }
}
