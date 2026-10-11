namespace IPAY.Application.DTOs.Auth
{
    public class CreateSellerRequestDto
    {
        /// <summary>Optional note for the reviewer, up to 500 characters.</summary>
        public string? Message { get; set; }
    }

    /// <summary>One seller request as shown to the requester and on the admin page.</summary>
    public class SellerRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>"Pending", "Approved" or "Rejected".</summary>
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
