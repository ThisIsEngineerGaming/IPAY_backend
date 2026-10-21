using IPAY.Application.DTOs.Shop;
using IPAY.Application.Validators.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Validators.Shop;

[TestFixture]
public sealed class CreateAdminProductDtoValidatorTests
{
    private static CreateAdminProductDto ValidRequest() => new()
    {
        Id = null, Name = "Keyboard", Price = 100, DiscountedPrice = 80,
        ImageUrl = "https://example.com/product.png", Manufacturer = "Example",
        CategoryId = 1, DiscountPercent = 20
    };

    [Test]
    public void ValidRequest_WithServerGeneratedId_IsAccepted()
    {
        var result = new CreateAdminProductDtoValidator().Validate(ValidRequest());
        Assert.That(result.IsValid, Is.True, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ExplicitId_WhenNotPositive_IsRejected(int id)
    {
        var dto = ValidRequest(); dto.Id = id;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Name_WhenEmptyOrWhitespace_IsRejected(string name)
    {
        var dto = ValidRequest(); dto.Name = name;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [Test]
    public void Name_WhenLongerThan120Characters_IsRejected()
    {
        var dto = ValidRequest(); dto.Name = new string('x', 121);
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(1.234)]
    public void Price_WhenInvalid_IsRejected(double price)
    {
        var dto = ValidRequest(); dto.Price = price;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [TestCase(-1)]
    [TestCase(101)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void DiscountPercent_WhenOutsideAllowedRange_IsRejected(double percent)
    {
        var dto = ValidRequest(); dto.DiscountPercent = percent;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [TestCase(-0.01)]
    [TestCase(100.01)]
    [TestCase(10.123)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void DiscountedPrice_WhenInvalid_IsRejected(double price)
    {
        var dto = ValidRequest(); dto.DiscountedPrice = price;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [Test]
    public void DiscountedPrice_WhenGreaterThanPrice_IsRejected()
    {
        var dto = ValidRequest(); dto.DiscountedPrice = 100.01;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [TestCase("")]
    [TestCase("not a url")]
    [TestCase("ftp://example.com/file.png")]
    public void ImageUrl_WhenInvalid_IsRejected(string url)
    {
        var dto = ValidRequest(); dto.ImageUrl = url;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [Test]
    public void ImageUrl_WhenLongerThan2048Characters_IsRejected()
    {
        var dto = ValidRequest(); dto.ImageUrl = "https://example.com/" + new string('x', 2050);
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Manufacturer_WhenEmptyOrWhitespace_IsRejected(string manufacturer)
    {
        var dto = ValidRequest(); dto.Manufacturer = manufacturer;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [Test]
    public void Manufacturer_WhenLongerThan100Characters_IsRejected()
    {
        var dto = ValidRequest(); dto.Manufacturer = new string('x', 101);
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void CategoryId_WhenNotPositive_IsRejected(int categoryId)
    {
        var dto = ValidRequest(); dto.CategoryId = categoryId;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.False);
    }

    [Test]
    public void BoundaryValues_AreAccepted()
    {
        var dto = ValidRequest();
        dto.Price = 1; dto.DiscountedPrice = 0; dto.DiscountPercent = 0;
        Assert.That(new CreateAdminProductDtoValidator().Validate(dto).IsValid, Is.True);
    }
}
