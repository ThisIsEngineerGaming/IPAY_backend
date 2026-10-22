namespace IPAY.Application.DTOs.Auth
{
    /// <summary>
    /// One row of the admin page's "Users" list. Deliberately has no password hash - the list is also
    /// shown to moderators, who must not be able to read credentials.
    /// </summary>
    //a dto which gives admin information about users
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        /// <summary>The role's name ("Customer", "Seller", "Moderator", "Admin"), not its number.</summary>
        public string Role { get; set; } = string.Empty;

        public bool IsBanned { get; set; }
    }
}
