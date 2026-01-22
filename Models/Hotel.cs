namespace TourismPlatform.Models
{
    public class Hotel
    {
        public int HotelId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? AttractionId { get; set; }
        public int DistrictId { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Attraction Attraction { get; set; }
        public District District { get; set; }

        // Navigation properties
        public ICollection<HotelRoomType> RoomTypes { get; set; } = new List<HotelRoomType>();
        public ICollection<HotelOrder> Orders { get; set; } = new List<HotelOrder>();
    }
}
