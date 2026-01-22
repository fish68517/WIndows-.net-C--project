namespace TourismPlatform.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public int? AttractionId { get; set; }
        public int? DiaryId { get; set; }
        public string Content { get; set; }
        public int? Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public User User { get; set; }
        public Attraction Attraction { get; set; }
        public TravelDiary Diary { get; set; }
    }
}
