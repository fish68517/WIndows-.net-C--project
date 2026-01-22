namespace TourismPlatform.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int UserId { get; set; }
        public int AttractionId { get; set; }
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public User User { get; set; }
        public Attraction Attraction { get; set; }
    }
}
