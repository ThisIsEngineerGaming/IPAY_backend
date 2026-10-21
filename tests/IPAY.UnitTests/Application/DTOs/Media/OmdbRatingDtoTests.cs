using System.Text.Json;
using IPAY.Application.DTOs.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.DTOs.Media;

[TestFixture]
public sealed class OmdbRatingDtoTests
{
    [Test]
    public void Serialize_UsesExpectedJsonPropertyNames()
    {
        var json = JsonSerializer.Serialize(new OmdbRatingDto { Source = "IMDb", Value = "8.2/10" });
        Assert.That(json, Does.Contain("\"Source\":\"IMDb\""));
        Assert.That(json, Does.Contain("\"Value\":\"8.2/10\""));
    }

    [Test]
    public void Deserialize_ReadsExpectedJsonPropertyNames()
    {
        var dto = JsonSerializer.Deserialize<OmdbRatingDto>(
            "{\"Source\":\"Rotten Tomatoes\",\"Value\":\"91%\"}");

        Assert.Multiple(() =>
        {
            Assert.That(dto!.Source, Is.EqualTo("Rotten Tomatoes"));
            Assert.That(dto.Value, Is.EqualTo("91%"));
        });
    }
}
