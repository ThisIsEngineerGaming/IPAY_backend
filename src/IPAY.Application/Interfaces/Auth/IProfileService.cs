using IPAY.Application.DTOs.Auth;

namespace IPAY.Application.Interfaces.Auth
{
    public enum ProfileFailure
    {
        None,
        /// <summary>The signed-in account no longer exists (or is banned).</summary>
        NotFound,
        /// <summary>Bad input, wrong password, address already taken... - the message is safe to show the person.</summary>
        Invalid
    }

    /// <summary>Outcome of a profile call: either a value or a message safe to show the user.</summary>
    public record ProfileResult<T>(T? Value, string? Error, ProfileFailure Failure = ProfileFailure.None)
    {
        public bool Succeeded => Error is null;
        public static ProfileResult<T> Ok(T value) => new(value, null);
        public static ProfileResult<T> Fail(string error) => new(default, error, ProfileFailure.Invalid);
        public static ProfileResult<T> NotFound() => new(default, "Account not found.", ProfileFailure.NotFound);
    }

    /// <summary>The "Your profile" page: view the account and change name, phone, password and email.</summary>
    public interface IProfileService
    {
        Task<ProfileResult<ProfileDto>> GetProfileAsync(int userId);

        /// <summary>Needs the current password (accounts without one, i.e. Google-only, skip that check).</summary>
        Task<ProfileResult<ProfileDto>> ChangeUsernameAsync(int userId, ChangeUsernameDto dto);

        /// <summary>Needs the current password (accounts without one, i.e. Google-only, skip that check).</summary>
        Task<ProfileResult<ProfileDto>> ChangePhoneAsync(int userId, ChangePhoneDto dto);

        /// <summary>
        /// Password change, option 1: the current password and the new one. Updates our stored hash and keeps the
        /// Firebase account's password in step.
        /// </summary>
        Task<ProfileResult<ProfileDto>> ChangePasswordAsync(int userId, ChangePasswordDto dto);

        /// <summary>
        /// Password change, option 2 (email reset), step 1: makes sure a Firebase account exists to send the
        /// reset email from. No current password needed. The client then asks Firebase to email the reset link.
        /// Changes nothing here.
        /// </summary>
        Task<ProfileResult<ProfileDto>> StartPasswordChangeAsync(int userId);

        /// <summary>
        /// Password change, option 2 (email reset), step 2: the person opened the Firebase reset email and chose
        /// the new password there. The Firebase ID token (from signing in with that new password) proves it, and
        /// only then is our stored hash updated.
        /// </summary>
        Task<ProfileResult<ProfileDto>> ConfirmPasswordChangeAsync(int userId, ConfirmPasswordChangeDto dto);

        /// <summary>
        /// "Forgot password?" (signed out), step 1: makes sure a Firebase account exists for this email so the
        /// reset email can really be sent. Never reveals whether the email belongs to an account: it always
        /// completes quietly, and Google-only, banned and unknown addresses are simply ignored.
        /// </summary>
        Task StartForgotPasswordAsync(string? email);

        /// <summary>
        /// "Forgot password?" (signed out), step 2: the person opened the Firebase reset email and chose the new
        /// password there. The Firebase ID token (from signing in with it) identifies the account and proves the
        /// mailbox was reached; only then is our stored hash updated. The account comes from the token, never
        /// from the request.
        /// </summary>
        Task<ProfileResult<bool>> ResetForgottenPasswordAsync(ConfirmPasswordChangeDto dto);

        /// <summary>Email change, step 1: password is right and the new address is free. Changes nothing.</summary>
        Task<ProfileResult<ProfileDto>> StartEmailChangeAsync(int userId, StartEmailChangeDto dto);

        /// <summary>
        /// Email change, step 2: the Firebase ID token proves the new address was verified (the person clicked
        /// the link). Switches the email and returns a fresh session, because the JWT carries the email.
        /// </summary>
        Task<ProfileResult<AuthResponse>> ConfirmEmailChangeAsync(int userId, ConfirmEmailChangeDto dto);
    }
}
