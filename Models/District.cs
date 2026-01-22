namespace TourismPlatform.Models
{
    public class District
    {
        public int DistrictId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
        public ICollection<Food> Foods { get; set; } = new List<Food>();
        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    }
}
