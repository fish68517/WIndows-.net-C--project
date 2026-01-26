namespace TourismPlatform.Models
{
    public enum HotelOrderStatus
    {
        PendingPay,
        Paid,
        Cancelled,
        ToUse,
        Used,
        CheckedIn,
        RefundRequested  // 【新增】申请退票中
    }

    public class HotelOrder
    {
        public int HotelOrderId { get; set; }
        public int UserId { get; set; }
        public int RoomTypeId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public HotelOrderStatus Status { get; set; } = HotelOrderStatus.PendingPay;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Foreign keys
        public User User { get; set; }
        public HotelRoomType RoomType { get; set; }

        // Navigation properties
        public VerifyCode VerifyCode { get; set; }
    }
}
