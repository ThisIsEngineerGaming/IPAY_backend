using IPAY.Application.Options;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Options;

[TestFixture]
public class OmdbOptionsTests
{
    [Test]
    public void Defaults_PointAtTheOfficialOmdbApiWithNoKey()
    {
        var options = new OmdbOptions();

        Assert.Multiple(() =>
        {
            Assert.That(OmdbOptions.SectionName, Is.EqualTo("Omdb"));
            Assert.That(options.ApiKey, Is.Empty);
            Assert.That(options.BaseUrl, Is.EqualTo("https://www.omdbapi.com"));
        });
    }

    [Test]
    public void Values_CanBeOverriddenFromConfiguration()
    {
        var options = new OmdbOptions { ApiKey = "secret", BaseUrl = "http://localhost:5000" };

        Assert.Multiple(() =>
        {
            Assert.That(options.ApiKey, Is.EqualTo("secret"));
            Assert.That(options.BaseUrl, Is.EqualTo("http://localhost:5000"));
        });
    }
}
