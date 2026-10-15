namespace IPAY.Application.DTOs.Shop
{
    public class OrderStatusHistoryDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public int? ChangedByUserId { get; set; }
        public string? Comment { get; set; }
    }
}
