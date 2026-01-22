namespace TourismPlatform.Services
{
    public interface IStatsService
    {
        Task<StatsData> GetStatsAsync();
    }

    public class StatsData
    {
        public int TotalUsers { get; set; }
        public int TotalAttractions { get; set; }
        public int TotalDiaries { get; set; }
        public int TotalTicketOrders { get; set; }
        public int TotalHotelOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingTicketOrders { get; set; }
        public int PendingHotelOrders { get; set; }
        public int TotalComments { get; set; }
        public int TotalFavorites { get; set; }
        public List<TopAttractionData> TopAttractions { get; set; }
    }

    public class TopAttractionData
    {
        public int AttractionId { get; set; }
        public string Name { get; set; }
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public int OrderCount { get; set; }
        public int TotalScore { get; set; }
    }
}
