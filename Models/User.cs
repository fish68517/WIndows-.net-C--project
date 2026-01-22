namespace TourismPlatform.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Nickname { get; set; }
        // --- 核心修改：添加 ? 标记为可空 ---
        public string? Bio { get; set; }       // 允许为 NULL
        public string? AvatarUrl { get; set; } // 允许为 NULL
        // ----------------------------------
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TravelDiary> TravelDiaries { get; set; } = new List<TravelDiary>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();
        public ICollection<HotelOrder> HotelOrders { get; set; } = new List<HotelOrder>();
    }
}
