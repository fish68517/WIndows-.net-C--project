using System.ComponentModel.DataAnnotations;
namespace TourismPlatform.Models
{
    public class HotelRoomType
    {
        [Key]
        public int RoomTypeId { get; set; }
        public int HotelId { get; set; }
        public string RoomTypeName { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }

        // Foreign key
        public Hotel Hotel { get; set; }

        // Navigation properties
        public ICollection<HotelOrder> Orders { get; set; } = new List<HotelOrder>();
    }
}
