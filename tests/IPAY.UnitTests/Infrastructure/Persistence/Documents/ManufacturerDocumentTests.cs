using IPAY.Domain.Entities.Shop;
using IPAY.Infrastructure.Persistence.Documents;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

[TestFixture]
public class ManufacturerDocumentTests
{
    [Test]
    public void DefaultEntity_CanBeMappedToDocumentAndBack()
    {
        DocumentMappingTestHelper.AssertDefaultMappingRoundTrip<ManufacturerDocument, Manufacturer>();
    }
}
