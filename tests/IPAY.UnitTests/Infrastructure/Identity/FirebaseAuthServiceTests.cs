using System.Reflection;
using System.Text.Json;
using IPAY.Infrastructure.Identity;
using NUnit.Framework;

namespace IPAY.UnitTests.Infrastructure.Identity;

[TestFixture]
public class FirebaseAuthServiceTests
{
    private static MethodInfo PrivateStatic(string name) =>
        typeof(FirebaseAuthService).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    [TestCase(123L, 123L)]
    [TestCase(123, 123L)]
    [TestCase(123.9, 123L)]
    [TestCase("123", 123L)]
    public void ToUnixSeconds_ParsesSupportedNumericClaimTypes(object input, long expected)
    {
        var actual = PrivateStatic("ToUnixSeconds").Invoke(null, new[] { input });
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void ToUnixSeconds_ReturnsNullForInvalidClaim()
    {
        Assert.That(PrivateStatic("ToUnixSeconds").Invoke(null, new object?[] { "not-a-number" }), Is.Null);
        Assert.That(PrivateStatic("ToUnixSeconds").Invoke(null, new object?[] { null }), Is.Null);
    }

    [Test]
    public void ToUnixSeconds_ParsesJsonNumber()
    {
        using var json = JsonDocument.Parse("1700000000");
        Assert.That(PrivateStatic("ToUnixSeconds").Invoke(null, new object?[] { json.RootElement }), Is.EqualTo(1700000000L));
    }

    [TestCase(true, true)]
    [TestCase("true", true)]
    [TestCase("TRUE", true)]
    [TestCase(false, false)]
    [TestCase("false", false)]
    [TestCase("1", false)]
    public void IsTrue_HandlesFirebaseClaimRepresentations(object input, bool expected)
    {
        Assert.That(PrivateStatic("IsTrue").Invoke(null, new[] { input }), Is.EqualTo(expected));
    }

    [Test]
    public void IsTrue_AcceptsJsonTrueOnly()
    {
        using var yes = JsonDocument.Parse("true");
        using var no = JsonDocument.Parse("false");
        Assert.That(PrivateStatic("IsTrue").Invoke(null, new object[] { yes.RootElement }), Is.EqualTo(true));
        Assert.That(PrivateStatic("IsTrue").Invoke(null, new object[] { no.RootElement }), Is.EqualTo(false));
    }
}
