using FluentValidation;

namespace IPAY.Application.Validators.Media
{
    /// Rules shared by the film / series / episode / genre validators.
    internal static class MediaRules
    {
        public const int MaxNameLength = 200;
        public const int MaxDescriptionLength = 5000;
        public const int MaxUrlLength = 2048;

        /// Empty is allowed (field not set); otherwise it must be an absolute http(s) URL.
        public static IRuleBuilderOptions<T, string> EmptyOrHttpUrl<T>(this IRuleBuilder<T, string> rule) =>
            rule.MaximumLength(MaxUrlLength)
                .Must(value => string.IsNullOrEmpty(value)
                    || (Uri.TryCreate(value, UriKind.Absolute, out var uri)
                        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
                .WithMessage("'{PropertyName}' must be empty or an absolute http(s) URL.");

        /// Every id must be positive and none may repeat.
        public static IRuleBuilderOptions<T, List<int>> PositiveDistinctIds<T>(this IRuleBuilder<T, List<int>> rule) =>
            rule.NotNull()
                .Must(ids => ids is null || ids.All(id => id > 0))
                .WithMessage("'{PropertyName}' must contain only positive ids.")
                .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
                .WithMessage("'{PropertyName}' must not contain duplicates.");

        public static IRuleBuilderOptions<T, double> ScoreFromZeroToTen<T>(this IRuleBuilder<T, double> rule) =>
            rule.InclusiveBetween(0, 10).WithMessage("'{PropertyName}' must be between 0 and 10.");

        /// First film ever shot is 1888; allow a few years ahead for announced titles.
        public static IRuleBuilderOptions<T, int> ReleaseYear<T>(this IRuleBuilder<T, int> rule) =>
            rule.InclusiveBetween(1888, DateTime.UtcNow.Year + 5)
                .WithMessage("'{PropertyName}' must be between 1888 and " + (DateTime.UtcNow.Year + 5) + ".");
    }
}
