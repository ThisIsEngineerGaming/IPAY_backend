using FluentValidation.TestHelper;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Validators.Media;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Validators.Media
{
    [TestFixture]
    public class MediaValidatorsTests
    {
        private static SaveFilmDto ValidFilm() => new()
        {
            Name = "The Matrix",
            Year = 1999,
            Rating = 8.7,
            PosterUrl = "https://example.com/poster.jpg",
            GenreIds = new List<int> { 1, 2 },
            ImdbId = "tt0133093"
        };

        [Test]
        public void Film_Valid_HasNoErrors() =>
            new SaveFilmDtoValidator().TestValidate(ValidFilm()).ShouldNotHaveAnyValidationErrors();

        [Test]
        public void Film_EmptyName_IsRejected()
        {
            var film = ValidFilm(); film.Name = " ";
            new SaveFilmDtoValidator().TestValidate(film).ShouldHaveValidationErrorFor(x => x.Name);
        }

        [TestCase(-0.1)]
        [TestCase(10.5)]
        public void Film_RatingOutsideZeroToTen_IsRejected(double rating)
        {
            var film = ValidFilm(); film.Rating = rating;
            new SaveFilmDtoValidator().TestValidate(film).ShouldHaveValidationErrorFor(x => x.Rating);
        }

        [Test]
        public void Film_YearBeforeCinema_IsRejected()
        {
            var film = ValidFilm(); film.Year = 1800;
            new SaveFilmDtoValidator().TestValidate(film).ShouldHaveValidationErrorFor(x => x.Year);
        }

        [TestCase("not a url")]
        [TestCase("javascript:alert(1)")]
        [TestCase("ftp://example.com/a.jpg")]
        public void Film_PosterUrlThatIsNotHttp_IsRejected(string url)
        {
            var film = ValidFilm(); film.PosterUrl = url;
            new SaveFilmDtoValidator().TestValidate(film).ShouldHaveValidationErrorFor(x => x.PosterUrl);
        }

        [Test]
        public void Film_EmptyUrls_AreAllowed()
        {
            var film = ValidFilm(); film.PosterUrl = ""; film.VideoUrl = "";
            new SaveFilmDtoValidator().TestValidate(film).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Film_DuplicateOrNonPositiveGenreIds_AreRejected()
        {
            var duplicate = ValidFilm(); duplicate.GenreIds = new List<int> { 1, 1 };
            var negative = ValidFilm(); negative.GenreIds = new List<int> { 0 };

            new SaveFilmDtoValidator().TestValidate(duplicate).ShouldHaveValidationErrorFor(x => x.GenreIds);
            new SaveFilmDtoValidator().TestValidate(negative).ShouldHaveValidationErrorFor(x => x.GenreIds);
        }

        [Test]
        public void Film_MalformedImdbId_IsRejected()
        {
            var film = ValidFilm(); film.ImdbId = "133093";
            new SaveFilmDtoValidator().TestValidate(film).ShouldHaveValidationErrorFor(x => x.ImdbId);
        }

        [Test]
        public void Series_Valid_HasNoErrors() =>
            new SaveSeriesDtoValidator().TestValidate(new SaveSeriesDto { Name = "Dark", Year = 2017, Rating = 8.7 })
                .ShouldNotHaveAnyValidationErrors();

        [Test]
        public void Series_EmptyName_IsRejected() =>
            new SaveSeriesDtoValidator().TestValidate(new SaveSeriesDto { Name = "", Year = 2017 })
                .ShouldHaveValidationErrorFor(x => x.Name);

        [Test]
        public void Episode_WithoutSeries_IsRejected() =>
            new SaveEpisodeDtoValidator().TestValidate(new SaveEpisodeDto { Name = "Pilot", SerialId = 0 })
                .ShouldHaveValidationErrorFor(x => x.SerialId);

        [Test]
        public void Episode_Valid_HasNoErrors() =>
            new SaveEpisodeDtoValidator().TestValidate(new SaveEpisodeDto { Name = "Pilot", SerialId = 3, Rating = 9 })
                .ShouldNotHaveAnyValidationErrors();

        [Test]
        public void Genre_EmptyName_IsRejected() =>
            new SaveGenreDtoValidator().TestValidate(new SaveGenreDto { Name = "" })
                .ShouldHaveValidationErrorFor(x => x.Name);

        [Test]
        public void Genre_Valid_HasNoErrors() =>
            new SaveGenreDtoValidator().TestValidate(new SaveGenreDto { Name = "Drama" })
                .ShouldNotHaveAnyValidationErrors();
    }
}
