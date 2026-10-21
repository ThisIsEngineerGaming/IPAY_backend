using IPAY.Domain.Entities.Users;
using IPAY.Infrastructure.Persistence.Documents;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

[TestFixture]
public class GuestDocumentTests
{
    [Test]
    public void DefaultEntity_CanBeMappedToDocumentAndBack()
    {
        DocumentMappingTestHelper.AssertDefaultMappingRoundTrip<GuestDocument, Guest>();
    }
}
