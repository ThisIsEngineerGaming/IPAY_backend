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
        /// Password change, step 1: the current password is right and a Firebase account exists to send the
        /// reset email from. The client then asks Firebase to email the reset link. Changes nothing here.
        /// </summary>
        Task<ProfileResult<ProfileDto>> StartPasswordChangeAsync(int userId, StartPasswordChangeDto dto);

        /// <summary>
        /// Password change, step 2: the person opened the Firebase reset email and chose the new password there.
        /// The Firebase ID token (from signing in with that new password) proves it, and only then is our
        /// stored hash updated. There is deliberately no way to change the password without this step.
        /// </summary>
        Task<ProfileResult<ProfileDto>> ConfirmPasswordChangeAsync(int userId, ConfirmPasswordChangeDto dto);

        /// <summary>Email change, step 1: password is right and the new address is free. Changes nothing.</summary>
        Task<ProfileResult<ProfileDto>> StartEmailChangeAsync(int userId, StartEmailChangeDto dto);

        /// <summary>
        /// Email change, step 2: the Firebase ID token proves the new address was verified (the person clicked
        /// the link). Switches the email and returns a fresh session, because the JWT carries the email.
        /// </summary>
        Task<ProfileResult<AuthResponse>> ConfirmEmailChangeAsync(int userId, ConfirmEmailChangeDto dto);
    }
}
