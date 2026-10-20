using IPAY.Application.DTOs.Auth;
using IPAY.Application.Validators.Auth;
using NUnit.Framework;

namespace IPAY.UnitTests.Application.Validators.Auth
{
    [TestFixture]
    public class ProfileValidatorsTests
    {
        // ---------- PhoneRules ----------

        [TestCase("+380123456789", "+380123456789")]
        [TestCase("+380 (12) 345-6789", "+380123456789")]
        [TestCase("  0123456789  ", "0123456789")]
        [TestCase("123.456.7890", "1234567890")]
        [TestCase("1234567", "1234567")]
        [TestCase("123456789012345", "123456789012345")]
        public void PhoneRules_TryNormalize_AcceptsAndCleansValidNumbers(string raw, string expected)
        {
            Assert.That(PhoneRules.TryNormalize(raw, out var normalized), Is.True);
            Assert.That(normalized, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("123456")]
        [TestCase("1234567890123456")]
        [TestCase("12345abc678")]
        [TestCase("+")]
        [TestCase("12/345 6789")]
        public void PhoneRules_TryNormalize_RejectsInvalidNumbers(string? raw)
        {
            Assert.That(PhoneRules.TryNormalize(raw, out var normalized), Is.False);
            Assert.That(normalized, Is.Empty);
        }

        // ---------- ChangeUsernameValidator ----------

        [TestCase("Alice", true)]
        [TestCase("Mary-Jane", true)]
        [TestCase("Олена Коваль", true)]
        [TestCase("Їжак", true)]
        [TestCase("Al", true)]
        [TestCase("A", false)]
        [TestCase("", false)]
        [TestCase("Alice1", false)]
        [TestCase("Bob_x", false)]
        public void ChangeUsernameValidator_ChecksTheNameRules(string name, bool valid)
        {
            var result = new ChangeUsernameValidator().Validate(new ChangeUsernameDto { NewName = name });

            Assert.That(result.IsValid, Is.EqualTo(valid));
        }

        [Test]
        public void ChangeUsernameValidator_RejectsNamesLongerThanFiftyCharacters()
        {
            var result = new ChangeUsernameValidator().Validate(new ChangeUsernameDto { NewName = new string('a', 51) });

            Assert.That(result.IsValid, Is.False);
        }

        // ---------- ChangePhoneValidator ----------

        [TestCase("+380123456789", true)]
        [TestCase("abc", false)]
        [TestCase("", false)]
        public void ChangePhoneValidator_ChecksThePhoneRules(string phone, bool valid)
        {
            var result = new ChangePhoneValidator().Validate(new ChangePhoneDto { NewPhone = phone });

            Assert.That(result.IsValid, Is.EqualTo(valid));
        }

        // ---------- password rules ----------

        [TestCase("NewPass1!", true)]
        [TestCase("Sh0rt!", false)]
        [TestCase("nouppercase1!", false)]
        [TestCase("NOLOWERCASE1!", false)]
        [TestCase("NoDigitsHere!", false)]
        [TestCase("NoSpecial123", false)]
        [TestCase("", false)]
        public void ResetForgottenPasswordValidator_ChecksPasswordStrength(string password, bool valid)
        {
            var result = new ResetForgottenPasswordValidator().Validate(
                new ConfirmPasswordChangeDto { IdToken = "token", NewPassword = password });

            Assert.That(result.IsValid, Is.EqualTo(valid));
        }

        [Test]
        public void ResetForgottenPasswordValidator_RequiresTheFirebaseToken()
        {
            var result = new ResetForgottenPasswordValidator().Validate(
                new ConfirmPasswordChangeDto { IdToken = "", NewPassword = "NewPass1!" });

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors[0].ErrorMessage, Does.Contain("Confirmation is missing"));
        }

        [Test]
        public void ChangePasswordValidator_RequiresTheCurrentPassword()
        {
            var result = new ChangePasswordValidator().Validate(
                new ChangePasswordDto { CurrentPassword = "", NewPassword = "NewPass1!" });

            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ChangePasswordValidator_RejectsReusingTheCurrentPassword()
        {
            var result = new ChangePasswordValidator().Validate(
                new ChangePasswordDto { CurrentPassword = "SamePass1!", NewPassword = "SamePass1!" });

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Select(e => e.ErrorMessage), Has.Some.Contain("different"));
        }

        [Test]
        public void ChangePasswordValidator_AcceptsAStrongDifferentPassword()
        {
            var result = new ChangePasswordValidator().Validate(
                new ChangePasswordDto { CurrentPassword = "OldPass1!", NewPassword = "NewPass1!" });

            Assert.That(result.IsValid, Is.True);
        }

        // ---------- email change validators ----------

        [TestCase("new@test.com", true)]
        [TestCase("not-an-email", false)]
        [TestCase("", false)]
        public void StartEmailChangeValidator_ChecksTheAddress(string email, bool valid)
        {
            var result = new StartEmailChangeValidator().Validate(new StartEmailChangeDto { NewEmail = email });

            Assert.That(result.IsValid, Is.EqualTo(valid));
        }

        [TestCase("new@test.com", true)]
        [TestCase("not-an-email", false)]
        [TestCase("", false)]
        public void ConfirmEmailChangeValidator_ChecksTheAddress(string email, bool valid)
        {
            var result = new ConfirmEmailChangeValidator().Validate(new ConfirmEmailChangeDto { NewEmail = email });

            Assert.That(result.IsValid, Is.EqualTo(valid));
        }

        [Test]
        public void EmailChangeValidators_RejectAddressesLongerThanOneHundredCharacters()
        {
            var tooLong = new string('a', 95) + "@t.com";

            Assert.That(new StartEmailChangeValidator().Validate(new StartEmailChangeDto { NewEmail = tooLong }).IsValid, Is.False);
            Assert.That(new ConfirmEmailChangeValidator().Validate(new ConfirmEmailChangeDto { NewEmail = tooLong }).IsValid, Is.False);
        }
    }
}
