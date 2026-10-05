using FluentValidation;
using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Validators.Media
{
    public class SaveSeriesDtoValidator : AbstractValidator<SaveSeriesDto>
    {
        public SaveSeriesDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(MediaRules.MaxNameLength);
            RuleFor(x => x.Description).MaximumLength(MediaRules.MaxDescriptionLength);
            RuleFor(x => x.Year).ReleaseYear();
            RuleFor(x => x.Rating).ScoreFromZeroToTen();
            RuleFor(x => x.Director).MaximumLength(MediaRules.MaxNameLength);
            RuleFor(x => x.AgeRating).MaximumLength(20);
            RuleFor(x => x.PosterUrl).EmptyOrHttpUrl();
            RuleFor(x => x.EpisodeIds).PositiveDistinctIds();
            RuleFor(x => x.GenreIds).PositiveDistinctIds();
        }
    }
}
