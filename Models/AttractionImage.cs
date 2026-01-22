using System.ComponentModel.DataAnnotations;
namespace TourismPlatform.Models
{
    public class AttractionImage
    {
        [Key]
        public int ImageId { get; set; }
        public int AttractionId { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }

        // Foreign key
        public Attraction Attraction { get; set; }
    }
}
