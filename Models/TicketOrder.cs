namespace TourismPlatform.Models
{
    public enum TicketOrderStatus
    {
        PendingPay,
        Paid,
        Cancelled,
        ToUse,
        Used,
        RefundRequested  // 【新增】申请退票中
    }

    public class TicketOrder
    {
        public int TicketOrderId { get; set; }
        public int UserId { get; set; }
        public int AttractionId { get; set; }
        public DateTime VisitDate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public TicketOrderStatus Status { get; set; } = TicketOrderStatus.PendingPay;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Foreign keys
        public User User { get; set; }
        public Attraction Attraction { get; set; }

        // Navigation properties
        public VerifyCode VerifyCode { get; set; }
    }
}
