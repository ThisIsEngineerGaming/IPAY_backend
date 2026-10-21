using IPAY.Infrastructure.Persistence.Documents;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

[TestFixture]
public class ProductRatingDocumentTests
{
    [Test]
    public void Document_CanBeCreatedWithDefaultValues()
    {
        DocumentMappingTestHelper.AssertDocumentCanBeCreated<ProductRatingDocument>();
    }

    [Test]
    public void Properties_RetainAssignedValues()
    {
        var createdAt = DateTime.UtcNow;
        var document = new ProductRatingDocument
        {
            ProductId = 12,
            UserId = "user-42",
            Value = 5,
            CreatedAt = createdAt
        };

        Assert.Multiple(() =>
        {
            Assert.That(document.ProductId, Is.EqualTo(12));
            Assert.That(document.UserId, Is.EqualTo("user-42"));
            Assert.That(document.Value, Is.EqualTo(5));
            Assert.That(document.CreatedAt, Is.EqualTo(createdAt));
        });
    }
}
