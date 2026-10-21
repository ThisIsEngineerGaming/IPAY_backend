using IPAY.Domain.Entities.Users;
using IPAY.Infrastructure.Persistence.Documents;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

[TestFixture]
public class SellerRequestDocumentTests
{
    [Test]
    public void DefaultEntity_CanBeMappedToDocumentAndBack()
    {
        DocumentMappingTestHelper.AssertDefaultMappingRoundTrip<SellerRequestDocument, SellerRequest>();
    }
}
