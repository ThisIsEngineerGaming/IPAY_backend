using IPAY.Domain.Enums;

namespace IPAY.Domain.Entities.Users
{
    /// <summary>A customer's request to become a seller, reviewed by an admin or moderator.</summary>
    public class SellerRequest
    {
        public int Id { get; set; }

        /// <summary>Id of the customer who asked.</summary>
        public int UserId { get; set; }

        // Copied at request time so the admin list doesn't need a second lookup per row.
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>Optional note from the customer (what they want to sell, etc.).</summary>
        public string Message { get; set; } = string.Empty;

        public SellerRequestStatus Status { get; set; } = SellerRequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
    }
}
