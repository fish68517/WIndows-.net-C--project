using TourismPlatform.Data;
using TourismPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace TourismPlatform.Services
{
    public class StatsService : IStatsService
    {
        private readonly MyDbContext _context;

        public StatsService(MyDbContext context)
        {
            _context = context;
        }

        public async Task<StatsData> GetStatsAsync()
        {
            var stats = new StatsData();

            // Calculate total users
            stats.TotalUsers = await _context.Users.CountAsync();

            // Calculate total attractions
            stats.TotalAttractions = await _context.Attractions.CountAsync();

            // Calculate total diaries
            stats.TotalDiaries = await _context.TravelDiaries.CountAsync();

            // Calculate total ticket orders
            stats.TotalTicketOrders = await _context.TicketOrders.CountAsync();

            // Calculate total hotel orders
            stats.TotalHotelOrders = await _context.HotelOrders.CountAsync();

            // Calculate total revenue (from paid and used orders)
            var ticketRevenue = await _context.TicketOrders
                .Where(o => o.Status == TicketOrderStatus.Paid || o.Status == TicketOrderStatus.Used)
                .SumAsync(o => o.TotalPrice);

            var hotelRevenue = await _context.HotelOrders
                .Where(o => o.Status == HotelOrderStatus.Paid || o.Status == HotelOrderStatus.Used || o.Status == HotelOrderStatus.CheckedIn)
                .SumAsync(o => o.TotalPrice);

            stats.TotalRevenue = ticketRevenue + hotelRevenue;

            // Calculate pending orders
            stats.PendingTicketOrders = await _context.TicketOrders
                .Where(o => o.Status == TicketOrderStatus.PendingPay)
                .CountAsync();

            stats.PendingHotelOrders = await _context.HotelOrders
                .Where(o => o.Status == HotelOrderStatus.PendingPay)
                .CountAsync();

            // Calculate total comments
            stats.TotalComments = await _context.Comments.CountAsync();

            // Calculate total favorites
            stats.TotalFavorites = await _context.Favorites.CountAsync();

            // Get top attractions by view count, favorites, and orders
            stats.TopAttractions = await GetTopAttractionsAsync();

            return stats;
        }

        private async Task<List<TopAttractionData>> GetTopAttractionsAsync()
        {
            var attractions = await _context.Attractions
                .Include(a => a.Favorites)
                .Include(a => a.TicketOrders)
                .Include(a => a.Comments)
                .ToListAsync();

            var topAttractions = attractions
                .Select(a => new TopAttractionData
                {
                    AttractionId = a.AttractionId,
                    Name = a.Name,
                    ViewCount = a.ViewCount,
                    FavoriteCount = a.Favorites?.Count ?? 0,
                    OrderCount = a.TicketOrders?.Count ?? 0,
                    TotalScore = a.Comments?.Sum(c => c.Rating ?? 0) ?? 0
                })
                .OrderByDescending(a => a.ViewCount)
                .ThenByDescending(a => a.FavoriteCount)
                .ThenByDescending(a => a.OrderCount)
                .Take(10)
                .ToList();

            return topAttractions;
        }
    }
}
