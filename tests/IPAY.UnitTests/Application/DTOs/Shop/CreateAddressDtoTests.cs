using IPAY.Application.DTOs.Shop;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.DTOs.Shop;

[TestFixture]
public class CreateAddressDtoTests
{
    [Test]
    public void NewDto_StartsWithEmptyStringsNotNulls()
    {
        var dto = new CreateAddressDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.Country, Is.Empty);
            Assert.That(dto.Street, Is.Empty);
            Assert.That(dto.City, Is.Empty);
        });
    }

    [Test]
    public void Properties_KeepTheValuesTheyAreGiven()
    {
        var dto = new CreateAddressDto { Country = "Ukraine", Street = "Deribasivska 1", City = "Odesa" };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Country, Is.EqualTo("Ukraine"));
            Assert.That(dto.Street, Is.EqualTo("Deribasivska 1"));
            Assert.That(dto.City, Is.EqualTo("Odesa"));
        });
    }
}
