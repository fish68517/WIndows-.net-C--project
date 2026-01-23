using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVerifyService _verifyService;

        public OrderService(IUnitOfWork unitOfWork, IVerifyService verifyService)
        {
            _unitOfWork = unitOfWork;
            _verifyService = verifyService;
        }

        public async Task<TicketOrder> CreateTicketOrderAsync(int userId, int attractionId, DateTime visitDate, int quantity)
        {
            var attraction = await _unitOfWork.Attractions.GetByIdAsync(attractionId);
            if (attraction == null) return null;

            // 打印订单信息
            Console.WriteLine($"用户 {userId} 购买了 {attraction.Name} 景点的 {quantity} 张门票，总价为 {attraction.TicketPrice * quantity} 元");
            var order = new TicketOrder
            {
                UserId = userId,
                AttractionId = attractionId,
                VisitDate = visitDate,
                Quantity = quantity,
                TotalPrice = attraction.TicketPrice * quantity,
                Status = TicketOrderStatus.PendingPay,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TicketOrders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return order;
        }

        public async Task<bool> PayTicketOrderAsync(int orderId)
        {
            var order = await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
            if (order == null || order.Status != TicketOrderStatus.PendingPay)
                return false;

            order.Status = TicketOrderStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            _unitOfWork.TicketOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            await _verifyService.GenerateCodeAsync(orderId, "Ticket");
            return true;
        }

        // ==========================================
        // 核心修复：手动加载关联数据 (VerifyCode 和 Attraction)
        // ==========================================
        public async Task<TicketOrder> GetTicketOrderAsync(int orderId)
        {
            // 1. 获取主订单
            var order = await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
            
            if (order != null)
            {
                // 2. 显式加载关联的 核销码
                var verifyCodes = await _unitOfWork.VerifyCodes.FindAsync(v => v.TicketOrderId == orderId);
                order.VerifyCode = verifyCodes.FirstOrDefault();

                // 3. 显式加载关联的 景点信息 (防止页面显示"未知景点")
                if (order.Attraction == null)
                {
                    order.Attraction = await _unitOfWork.Attractions.GetByIdAsync(order.AttractionId);
                }
            }
            return order;
        }

        // public async Task<TicketOrder> GetTicketOrderAsync(int orderId)
        // {
        //     return await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
        // }

        public async Task<IEnumerable<TicketOrder>> GetUserTicketOrdersAsync(int userId)
        {
            return await _unitOfWork.TicketOrders.FindAsync(o => o.UserId == userId);
        }

        public async Task<HotelOrder> CreateHotelOrderAsync(int userId, int roomTypeId, DateTime checkIn, DateTime checkOut)
        {
            var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(roomTypeId);
            if (roomType == null)
                return null;

            var nights = (checkOut - checkIn).Days;
            var order = new HotelOrder
            {
                UserId = userId,
                RoomTypeId = roomTypeId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = roomType.PricePerNight * nights,
                Status = HotelOrderStatus.PendingPay,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HotelOrders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return order;
        }

        public async Task<bool> PayHotelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
            if (order == null || order.Status != HotelOrderStatus.PendingPay)
                return false;

            order.Status = HotelOrderStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            _unitOfWork.HotelOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            // Generate verify code
            await _verifyService.GenerateCodeAsync(orderId, "Hotel");

            return true;
        }

        // ==========================================
        // 顺便修复：Hotel 也要加载 VerifyCode
        // ==========================================
        public async Task<HotelOrder> GetHotelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
            if (order != null)
            {
                var verifyCodes = await _unitOfWork.VerifyCodes.FindAsync(v => v.HotelOrderId == orderId);
                order.VerifyCode = verifyCodes.FirstOrDefault();
                
                if (order.RoomType == null)
                {
                    order.RoomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(order.RoomTypeId);
                     // 如果需要酒店名，还得继续查 Hotel，这里暂略
                }
            }
            return order;
        }

        // public async Task<HotelOrder> GetHotelOrderAsync(int orderId)
        // {
        //     return await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
        // }

        public async Task<IEnumerable<HotelOrder>> GetUserHotelOrdersAsync(int userId)
        {
            return await _unitOfWork.HotelOrders.FindAsync(o => o.UserId == userId);
        }
    }
}
