using FluentValidation;
using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Validators.Media
{
    public class SaveEpisodeDtoValidator : AbstractValidator<SaveEpisodeDto>
    {
        public SaveEpisodeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(MediaRules.MaxNameLength);
            RuleFor(x => x.Description).MaximumLength(MediaRules.MaxDescriptionLength);
            RuleFor(x => x.Rating).ScoreFromZeroToTen();
            RuleFor(x => x.Director).MaximumLength(MediaRules.MaxNameLength);
            RuleFor(x => x.PosterUrl).EmptyOrHttpUrl();
            RuleFor(x => x.VideoUrl).EmptyOrHttpUrl();
            RuleFor(x => x.SerialId).GreaterThan(0).WithMessage("'Serial Id' must reference a series.");
        }
    }
}
