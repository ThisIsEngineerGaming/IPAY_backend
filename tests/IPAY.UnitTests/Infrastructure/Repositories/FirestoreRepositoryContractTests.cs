using System.Runtime.CompilerServices;
using IPAY.Infrastructure.Persistence;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Repositories;

/// <summary>
/// Tests the one rule FirestoreRepository enforces without touching Firestore: the entity type needs a public
/// 'Id' property. (The previous version of this fixture only asserted facts about private classes declared inside
/// the test itself, so it could never fail and covered nothing.) Real I/O is covered by FirestoreRepositoryTests.
/// </summary>
[TestFixture]
public class FirestoreRepositoryContractTests
{
    private sealed class EntityWithId
    {
        public int Id { get; set; }
    }

    private sealed class EntityWithoutId
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class Document;

    [Test]
    public void EntityWithoutIdProperty_IsRejectedWhenTheRepositoryTypeInitialises()
    {
        var ex = Assert.Throws<TypeInitializationException>(() =>
            RuntimeHelpers.RunClassConstructor(typeof(FirestoreRepository<EntityWithoutId, Document>).TypeHandle));

        Assert.That(ex!.InnerException, Is.TypeOf<InvalidOperationException>());
        Assert.That(ex.InnerException!.Message, Does.Contain("EntityWithoutId").And.Contain("'Id'"));
    }

    [Test]
    public void EntityWithIdProperty_InitialisesWithoutError()
    {
        Assert.DoesNotThrow(() =>
            RuntimeHelpers.RunClassConstructor(typeof(FirestoreRepository<EntityWithId, Document>).TypeHandle));
    }
}
