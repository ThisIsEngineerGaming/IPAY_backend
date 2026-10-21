using System.Reflection;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Persistence.Documents;

internal static class DocumentMappingTestHelper
{
    public static void AssertDefaultMappingRoundTrip<TDocument, TEntity>()
        where TDocument : new()
        where TEntity : new()
    {
        var entity = new TEntity();
        var fromEntity = typeof(TDocument).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "FromEntity" &&
                              method.GetParameters().Length == 1 &&
                              method.GetParameters()[0].ParameterType == typeof(TEntity));

        var document = (TDocument?)fromEntity.Invoke(null, new object?[] { entity });
        Assert.That(document, Is.Not.Null, "FromEntity should return a document.");

        var toEntity = typeof(TDocument).GetMethod("ToEntity", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(toEntity, Is.Not.Null, "Document should expose ToEntity.");
        var mappedEntity = toEntity!.Invoke(document, null);
        Assert.That(mappedEntity, Is.InstanceOf<TEntity>());
    }

    public static void AssertDocumentCanBeCreated<TDocument>() where TDocument : new()
    {
        var document = new TDocument();
        Assert.That(document, Is.Not.Null);
    }
}
