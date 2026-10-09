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

        public static IRuleBuilderOptions<T, string> ValidPhone<T>(this IRuleBuilder<T, string> rule) =>
            rule
                .NotEmpty().WithMessage("Phone number is required.")
                .Must(phone => PhoneRules.TryNormalize(phone, out _))
                .WithMessage("Enter a valid phone number with 7-15 digits, e.g. +380 12 345 6789.");
    }

    public static class PhoneRules
    {
        /// <summary>
        /// Accepts what people type (spaces, dashes, dots, brackets, an optional leading +) and returns the
        /// compact form we store: an optional + followed by 7-15 digits (the E.164 limit).
        /// </summary>
        public static bool TryNormalize(string? raw, out string normalized)
        {
            normalized = string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            var text = raw.Trim();
            var hasPlus = text.StartsWith('+');
            if (hasPlus) text = text[1..];

            var digits = new System.Text.StringBuilder();
            foreach (var c in text)
            {
                if (c is >= '0' and <= '9') digits.Append(c);
                else if (c is ' ' or '-' or '.' or '(' or ')') continue;
                else return false;
            }

            if (digits.Length is < 7 or > 15) return false;

            normalized = (hasPlus ? "+" : string.Empty) + digits;
            return true;
        }
    }

    public class ChangeUsernameValidator : AbstractValidator<ChangeUsernameDto>
    {
        public ChangeUsernameValidator()
        {
            RuleFor(x => x.NewName).ValidName();
        }
    }

    public class ChangePhoneValidator : AbstractValidator<ChangePhoneDto>
    {
        public ChangePhoneValidator()
        {
            RuleFor(x => x.NewPhone).ValidPhone();
        }
    }

    public class StartPasswordChangeValidator : AbstractValidator<StartPasswordChangeDto>
    {
        public StartPasswordChangeValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");
        }
    }

    /// <summary>Signed-in password change, step 2: needs the current password.</summary>
    public class ConfirmPasswordChangeValidator : AbstractValidator<ConfirmPasswordChangeDto>
    {
        public ConfirmPasswordChangeValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Confirmation is missing. Please try again.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword).StrongPassword();

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current one.");
        }
    }

    /// <summary>
    /// "Forgot password" step 2. Same DTO as above, but there is no current password (that is the point), so it
    /// is NOT registered in DI next to ConfirmPasswordChangeValidator; ProfileService uses it directly.
    /// </summary>
    public class ResetForgottenPasswordValidator : AbstractValidator<ConfirmPasswordChangeDto>
    {
        public ResetForgottenPasswordValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Confirmation is missing. Please try again.");

            RuleFor(x => x.NewPassword).StrongPassword();
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
