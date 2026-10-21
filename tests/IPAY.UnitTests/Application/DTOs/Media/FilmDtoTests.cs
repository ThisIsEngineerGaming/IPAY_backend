using IPAY.Application.DTOs.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.DTOs.Media;

[TestFixture]
public sealed class FilmDtoTests
{
    [Test]
    public void Defaults_AreInitializedAsExpected()
    {
        var dto = new FilmDto();
        Assert.Multiple(() =>
        {
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.Description, Is.Empty);
            Assert.That(dto.Director, Is.Empty);
            Assert.That(dto.AgeRating, Is.Empty);
            Assert.That(dto.PosterUrl, Is.Empty);
            Assert.That(dto.VideoUrl, Is.Empty);
            Assert.That(dto.GenreIds, Is.Empty);
            Assert.That(dto.ImdbId, Is.Empty);
        });
    }

    [Test]
    public void Properties_RoundTripAssignedValues()
    {
        var created = new DateTime(2024, 5, 1, 12, 30, 0, DateTimeKind.Utc);
        var dto = new FilmDto
        {
            Id = 42, Name = "Example", Description = "Description", Year = 2024,
            Rating = 8.4, Director = "Director", AgeRating = "PG-13",
            PosterUrl = "https://example.com/poster.jpg", VideoUrl = "https://example.com/movie",
            GenreIds = new List<int> { 2, 5 }, ImdbId = "tt1234567", CreatedAt = created
        };
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(42));
            Assert.That(dto.Name, Is.EqualTo("Example"));
            Assert.That(dto.Description, Is.EqualTo("Description"));
            Assert.That(dto.Year, Is.EqualTo(2024));
            Assert.That(dto.Rating, Is.EqualTo(8.4));
            Assert.That(dto.Director, Is.EqualTo("Director"));
            Assert.That(dto.AgeRating, Is.EqualTo("PG-13"));
            Assert.That(dto.PosterUrl, Is.EqualTo("https://example.com/poster.jpg"));
            Assert.That(dto.VideoUrl, Is.EqualTo("https://example.com/movie"));
            Assert.That(dto.GenreIds, Is.EqualTo(new[] { 2, 5 }));
            Assert.That(dto.ImdbId, Is.EqualTo("tt1234567"));
            Assert.That(dto.CreatedAt, Is.EqualTo(created));
        });
    }
}
