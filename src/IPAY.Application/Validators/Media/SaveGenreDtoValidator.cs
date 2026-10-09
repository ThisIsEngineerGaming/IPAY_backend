using FluentValidation;
using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Validators.Media
{
    public class SaveGenreDtoValidator : AbstractValidator<SaveGenreDto>
    {
        public SaveGenreDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
            RuleFor(x => x.FilmIds).PositiveDistinctIds();
            RuleFor(x => x.SerialIds).PositiveDistinctIds();
        }
    }
}
