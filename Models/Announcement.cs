namespace TourismPlatform.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
