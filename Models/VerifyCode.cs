namespace TourismPlatform.Models
{
    public enum VerifyCodeStatus
    {
        Unused,
        Used
    }

    public enum OrderType
    {
        Ticket,
        Hotel
    }

    public class VerifyCode
    {
        public int VerifyCodeId { get; set; }
        public string Code { get; set; }
        public int? TicketOrderId { get; set; }
        public int? HotelOrderId { get; set; }
        public OrderType OrderType { get; set; }
        public VerifyCodeStatus Status { get; set; } = VerifyCodeStatus.Unused;
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public TicketOrder TicketOrder { get; set; }
        public HotelOrder HotelOrder { get; set; }
    }
}
