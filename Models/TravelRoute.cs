using System.ComponentModel.DataAnnotations;
namespace TourismPlatform.Models
{
    public class TravelRoute
    {
        [Key]
        public int RouteId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Itinerary { get; set; }
        public string AttractionIntroduction { get; set; }
        public string TransportSuggestion { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
