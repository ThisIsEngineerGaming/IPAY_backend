using IPAY.Domain.Entities.Media;
using IPAY.Infrastructure.Persistence.Documents;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

[TestFixture]
public class FilmDocumentTests
{
    [Test]
    public void DefaultEntity_CanBeMappedToDocumentAndBack()
    {
        DocumentMappingTestHelper.AssertDefaultMappingRoundTrip<FilmDocument, Film>();
    }
}
