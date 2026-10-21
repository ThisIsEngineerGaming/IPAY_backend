using System.Net.Sockets;
using Google.Api.Gax;
using Google.Cloud.Firestore;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure;

/// <summary>
/// Base class for tests that talk to a real Firestore through the local emulator
/// (<c>firebase emulators:start --only firestore</c>).
/// <para>
/// The emulator is looked up in FIRESTORE_EMULATOR_HOST, falling back to 127.0.0.1:8080 (the default port).
/// When nothing is listening the whole fixture is reported as Ignored with an explanation, so a plain
/// <c>dotnet test</c> stays green; start the emulator to actually run (and get coverage from) these tests.
/// It never talks to production: the emulator is required, not optional.
/// </para>
/// </summary>
public abstract class FirestoreEmulatorTestBase
{
    public const string EmulatorHostVariable = "FIRESTORE_EMULATOR_HOST";
    private const string DefaultEmulatorHost = "127.0.0.1:8080";
    private const string TestProjectId = "ipay-unit-tests";

    private readonly List<string> _collections = new();

    protected FirestoreDb Db { get; private set; } = null!;

    [OneTimeSetUp]
    public void ConnectToEmulator()
    {
        var host = Environment.GetEnvironmentVariable(EmulatorHostVariable);
        if (string.IsNullOrWhiteSpace(host)) host = DefaultEmulatorHost;

        if (!IsReachable(host))
            Assert.Ignore($"Firestore emulator not reachable at {host}. Start it with " +
                          "'firebase emulators:start --only firestore' (and set FIRESTORE_EMULATOR_HOST if the port differs).");

        Environment.SetEnvironmentVariable(EmulatorHostVariable, host);
        Db = new FirestoreDbBuilder
        {
            ProjectId = TestProjectId,
            EmulatorDetection = EmulatorDetection.EmulatorOnly
        }.Build();
    }

    /// <summary>A collection name no other test (or earlier run) uses; removed again after the fixture.</summary>
    protected string UniqueCollection(string prefix)
    {
        var name = $"{prefix}_{Guid.NewGuid():N}";
        _collections.Add(name);
        return name;
    }

    [OneTimeTearDown]
    public async Task CleanUpCollections()
    {
        if (Db is null) return;

        foreach (var name in _collections)
        {
            var snapshot = await Db.Collection(name).GetSnapshotAsync();
            foreach (var document in snapshot.Documents)
                await document.Reference.DeleteAsync();

            // FirestoreIdGenerator keeps its counter in _counters/{collection}.
            await Db.Collection("_counters").Document(name).DeleteAsync();
        }
    }

    private static bool IsReachable(string hostAndPort)
    {
        var separator = hostAndPort.LastIndexOf(':');
        if (separator <= 0 || !int.TryParse(hostAndPort[(separator + 1)..], out var port)) return false;
        var host = hostAndPort[..separator];

        try
        {
            using var client = new TcpClient();
            return client.ConnectAsync(host, port).Wait(TimeSpan.FromSeconds(2)) && client.Connected;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
