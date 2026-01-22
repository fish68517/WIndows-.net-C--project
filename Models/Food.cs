namespace TourismPlatform.Models
{
    public class Food
    {
        public int FoodId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? AttractionId { get; set; }
        public int? DistrictId { get; set; }
        public decimal ReferencePrice { get; set; }
        public string ContactPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Attraction Attraction { get; set; }
        public District District { get; set; }
    }
}
