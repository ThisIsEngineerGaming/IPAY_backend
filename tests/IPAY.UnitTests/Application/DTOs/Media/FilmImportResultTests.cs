using IPAY.Application.DTOs.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.DTOs.Media;

[TestFixture]
public sealed class FilmImportResultTests
{
    [Test]
    public void Constructor_PreservesStatusFilmAndUnmatchedGenres()
    {
        var film = new FilmDto { Id = 7, Name = "Imported" };
        var unmatched = new[] { "Mystery", "Short" };
        var result = new FilmImportResult(FilmImportStatus.Imported, film, unmatched);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(FilmImportStatus.Imported));
            Assert.That(result.Film, Is.SameAs(film));
            Assert.That(result.UnmatchedGenres, Is.EqualTo(unmatched));
        });
    }

    [Test]
    public void OptionalConstructorArguments_DefaultToNull()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new FilmImportResult(FilmImportStatus.NotFoundInOmdb).Film, Is.Null);
            Assert.That(new FilmImportResult(FilmImportStatus.AlreadyImported).UnmatchedGenres, Is.Null);
        });
    }
}
