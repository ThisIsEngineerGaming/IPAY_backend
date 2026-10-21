using System.Reflection;
using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using IPAY.Infrastructure.Identity;
using IPAY.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Identity;

/// <summary>
/// Offline tests for FirebaseAuthService: the constructor and the token-rejection paths that Firebase's SDK
/// decides on locally (empty / malformed tokens), plus the claim-parsing helpers. Nothing here calls Google.
/// The service reuses FirebaseApp.DefaultInstance, so a default app with a static access token (no key file,
/// no token refresh) is registered once for the fixture.
/// </summary>
[TestFixture]
public class FirebaseAuthServiceTokenTests
{
    private static MethodInfo PrivateStatic(string name) =>
        typeof(FirebaseAuthService).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static FirebaseAuthService Create(bool requireEmailVerification = true) =>
        new(Options.Create(new FirebaseOptions
            {
                ProjectId = "ipay-unit-tests",
                RequireEmailVerification = requireEmailVerification
            }),
            NullLogger<FirebaseAuthService>.Instance);

    [OneTimeSetUp]
    public void RegisterOfflineDefaultFirebaseApp()
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromAccessToken("unit-test-token"),
                ProjectId = "ipay-unit-tests"
            });
        }
    }

    [Test]
    public void Constructor_ExposesRequireEmailVerificationFromOptions()
    {
        Assert.That(Create(requireEmailVerification: true).RequireEmailVerification, Is.True);
        Assert.That(Create(requireEmailVerification: false).RequireEmailVerification, Is.False);
    }

    [Test]
    public async Task VerifyIdTokenAsync_WithEmptyToken_ReturnsNull()
    {
        Assert.That(await Create().VerifyIdTokenAsync(string.Empty), Is.Null);
    }

    [Test]
    public async Task VerifyIdTokenAsync_WithMalformedToken_ReturnsNull()
    {
        Assert.That(await Create().VerifyIdTokenAsync("this-is-not-a-jwt"), Is.Null);
    }

    [Test]
    public void ToUnixSeconds_RejectsNonNumericJsonAndFractions()
    {
        using var text = JsonDocument.Parse("\"123\"");
        using var fraction = JsonDocument.Parse("1.5");
        using var boolean = JsonDocument.Parse("true");

        Assert.Multiple(() =>
        {
            // A JSON *string* is not a JSON number, even though a plain .NET string "123" is accepted.
            Assert.That(PrivateStatic("ToUnixSeconds").Invoke(null, new object?[] { text.RootElement }), Is.Null);
            Assert.That(PrivateStatic("ToUnixSeconds").Invoke(null, new object?[] { fraction.RootElement }), Is.Null);
            Assert.That(PrivateStatic("ToUnixSeconds").Invoke(null, new object?[] { boolean.RootElement }), Is.Null);
        });
    }

    [Test]
    public void IsTrue_RejectsNonTrueJsonValuesAndUnknownTypes()
    {
        using var text = JsonDocument.Parse("\"true\"");
        using var nothing = JsonDocument.Parse("null");

        Assert.Multiple(() =>
        {
            Assert.That(PrivateStatic("IsTrue").Invoke(null, new object?[] { text.RootElement }), Is.EqualTo(false));
            Assert.That(PrivateStatic("IsTrue").Invoke(null, new object?[] { nothing.RootElement }), Is.EqualTo(false));
            Assert.That(PrivateStatic("IsTrue").Invoke(null, new object?[] { null }), Is.EqualTo(false));
            Assert.That(PrivateStatic("IsTrue").Invoke(null, new object?[] { 1 }), Is.EqualTo(false));
        });
    }
}
