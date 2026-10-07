using FluentValidation;
using IPAY.Application.DTOs.Auth;

namespace IPAY.Application.Validators.Auth
{
    /// <summary>Same password / name rules as registration, shared by the profile validators.</summary>
    public static class ProfileRules
    {
        public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
            rule
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule) =>
            rule
                .NotEmpty().WithMessage("Name is required.")
                .Must(name => name.Trim().Length is >= 2 and <= 50).WithMessage("Name must be 2-50 characters.")
                .Matches(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ\s\-]+$").WithMessage("Name can only contain letters, spaces and hyphens.");
    }

    public class ChangeUsernameValidator : AbstractValidator<ChangeUsernameDto>
    {
        public ChangeUsernameValidator()
        {
            RuleFor(x => x.NewName).ValidName();
        }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword).StrongPassword();

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current one.");
        }
    }

    public class StartEmailChangeValidator : AbstractValidator<StartEmailChangeDto>
    {
        public StartEmailChangeValidator()
        {
            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage("New email is required.")
                .EmailAddress().WithMessage("Enter a valid email address.")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Enter your current password.");
        }
    }

    public class ConfirmEmailChangeValidator : AbstractValidator<ConfirmEmailChangeDto>
    {
        public ConfirmEmailChangeValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Verification token is missing. Please try again.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Enter your current password.");
        }
    }
}
