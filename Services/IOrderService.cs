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

        // 【新增】退票/取消订单方法
        Task<bool> RefundTicketOrderAsync(int orderId, int userId);

        // 【新增】退票/取消订单方法
        Task<bool> RefundHotelOrderAsync(int orderId, int userId);

        // 【新增】管理员相关方法
        Task<List<TicketOrder>> GetAllTicketOrdersAsync(); // 获取所有票务订单
        Task<bool> AdminApproveRefundAsync(int orderId);   // 同意退票
        Task<bool> AdminRejectRefundAsync(int orderId);    // 拒绝退票
        Task<bool> AdminDeleteOrderAsync(int orderId);     // 删除订单

        Task<bool> AdminApproveHotelRefundAsync(int orderId);   // 同意酒店退款
        Task<bool> AdminRejectHotelRefundAsync(int orderId);    // 拒绝酒店退款
        Task<bool> AdminDeleteHotelOrderAsync(int orderId);     // 删除酒店订单

        // 【新增】同时获取所有门票和酒店订单
        Task<(List<TicketOrder> Tickets, List<HotelOrder> Hotels)> GetAllTicketAndHotelOrdersAsync();
    }
}
