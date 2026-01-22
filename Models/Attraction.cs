namespace TourismPlatform.Models
{
    public class Attraction
    {
        public int AttractionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DistrictId { get; set; }
        public int CategoryId { get; set; }
        public decimal TicketPrice { get; set; }
        public string OpeningHours { get; set; }
        public string Address { get; set; }
        public string TransportInfo { get; set; }
        public string ContactPhone { get; set; }
        public int ViewCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public District District { get; set; }
        public AttractionCategory Category { get; set; }

        // Navigation properties
        public ICollection<AttractionImage> Images { get; set; } = new List<AttractionImage>();
        public ICollection<Food> Foods { get; set; } = new List<Food>();
        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
