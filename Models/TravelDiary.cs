
using System.ComponentModel.DataAnnotations;
namespace TourismPlatform.Models
{
    public class TravelDiary
    {
        [Key]
        public int DiaryId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public User User { get; set; }

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
