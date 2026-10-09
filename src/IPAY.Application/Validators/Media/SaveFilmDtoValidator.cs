using FluentValidation;
using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Validators.Media
{
    public class SaveFilmDtoValidator : AbstractValidator<SaveFilmDto>
    {
        public SaveFilmDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(MediaRules.MaxNameLength);
            RuleFor(x => x.Description).MaximumLength(MediaRules.MaxDescriptionLength);
            RuleFor(x => x.Year).ReleaseYear();
            RuleFor(x => x.Rating).ScoreFromZeroToTen();
            RuleFor(x => x.Director).MaximumLength(MediaRules.MaxNameLength);
            RuleFor(x => x.AgeRating).MaximumLength(20);
            RuleFor(x => x.PosterUrl).EmptyOrHttpUrl();
            RuleFor(x => x.VideoUrl).EmptyOrHttpUrl();
            RuleFor(x => x.GenreIds).PositiveDistinctIds();
            RuleFor(x => x.ImdbId)
                .Matches(@"^tt\d{7,9}$").When(x => !string.IsNullOrEmpty(x.ImdbId))
                .WithMessage("'Imdb Id' must look like tt0133093.");
        }
    }
}
