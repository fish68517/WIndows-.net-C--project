using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IOrderService
    {
        // Ticket Orders
        Task<TicketOrder> CreateTicketOrderAsync(int userId, int attractionId, DateTime visitDate, int quantity);
        Task<bool> PayTicketOrderAsync(int orderId);
        Task<TicketOrder> GetTicketOrderAsync(int orderId);
        Task<IEnumerable<TicketOrder>> GetUserTicketOrdersAsync(int userId);

        // Hotel Orders
        Task<HotelOrder> CreateHotelOrderAsync(int userId, int roomTypeId, DateTime checkIn, DateTime checkOut);
        Task<bool> PayHotelOrderAsync(int orderId);
        Task<HotelOrder> GetHotelOrderAsync(int orderId);
        Task<IEnumerable<HotelOrder>> GetUserHotelOrdersAsync(int userId);
    }
}
