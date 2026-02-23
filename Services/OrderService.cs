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

        // =========================================================
        // 🟢 修复：获取用户订单时，必须加载 Attraction 关联数据
        // =========================================================
        public async Task<IEnumerable<TicketOrder>> GetUserTicketOrdersAsync(int userId)
        {
            // 1. 使用 GetAllAsync 加载 Attraction (可能还有 VerifyCode)
            // 这样 order.Attraction 就不为 null 了
            var allOrders = await _unitOfWork.TicketOrders.GetAllAsync(
                o => o.Attraction, 
                o => o.VerifyCode // 如果列表页将来要显示核销码，最好也带上
            );

            // 2. 在内存中筛选出当前用户的订单，并按时间倒序
            return allOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);
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

        // =========================================================
        // 🟢 修复：获取用户酒店订单时，加载 Hotel/RoomType 数据
        // =========================================================
        public async Task<IEnumerable<HotelOrder>> GetUserHotelOrdersAsync(int userId)
        {
            // 加载 RoomType 以及 RoomType.Hotel
            var allOrders = await _unitOfWork.HotelOrders.GetAllAsync(
                o => o.RoomType,
                o => o.RoomType.Hotel,
                o => o.VerifyCode
            );

            return allOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);
        }


        // 【新增】退票逻辑实现
        // 【修改】用户申请退票：状态改为 RefundRequested
    public async Task<bool> RefundTicketOrderAsync(int orderId, int userId)
    {
        var order = await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId) return false;

        // 只有 "已支付" 或 "待使用" 的订单可以申请退票
        // if (order.Status == TicketOrderStatus.Paid || order.Status == TicketOrderStatus.ToUse)
        // {
        //     order.Status = TicketOrderStatus.RefundRequested; // 变更为申请中
        //     _unitOfWork.TicketOrders.Update(order);
        //     await _unitOfWork.SaveChangesAsync();
        //     return true;
        // }
        // return false;

        order.Status = TicketOrderStatus.Cancelled; // 变更为申请中
            _unitOfWork.TicketOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
    }

    // 【新增】管理员：获取所有订单
    public async Task<List<TicketOrder>> GetAllTicketOrdersAsync()
    {
        // 联表查询用户和景点
        var orders = await _unitOfWork.TicketOrders.GetAllAsync(
            o => o.User, 
            o => o.Attraction
        );
        return orders.OrderByDescending(o => o.CreatedAt).ToList();
    }



    // 【新增】管理员：同意退票 -> 设为 Cancelled
    public async Task<bool> AdminApproveRefundAsync(int orderId)
    {
        var order = await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
        if (order == null) return false;

        order.Status = TicketOrderStatus.Cancelled; // 同意后变已取消
        _unitOfWork.TicketOrders.Update(order);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // 【新增】管理员：拒绝退票 -> 恢复为 ToUse
    public async Task<bool> AdminRejectRefundAsync(int orderId)
    {
        var order = await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
        if (order == null) return false;

        order.Status = TicketOrderStatus.ToUse; // 拒绝后恢复为待使用
        _unitOfWork.TicketOrders.Update(order);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // // 【新增】管理员：删除订单 (仅限已取消状态)
    // public async Task<bool> AdminDeleteOrderAsync(int orderId)
    // {
    //     var order = await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
    //     if (order == null) return false;

    //     // 只有 "已取消" 的订单允许删除
    //     if (order.Status == TicketOrderStatus.Cancelled)
    //     {
    //         _unitOfWork.TicketOrders.Remove(order);
    //         await _unitOfWork.SaveChangesAsync();
    //         return true;
    //     }
    //     return false;
    // }

    // 2. 修复门票订单删除 (预防同样的错误)
    // public async Task<bool> AdminDeleteOrderAsync(int id)
    // {
    //     var order = await _unitOfWork.TicketOrders.FindAsync(id);
    //     if (order == null) return false;

    //     // 【核心修复】 先删除关联的门票核销码
    //     var verifyCode = await _unitOfWork.VerifyCodes
    //                                 .FirstOrDefaultAsync(v => v.TicketOrderId == id);
    //     if (verifyCode != null)
    //     {
    //         _unitOfWork.VerifyCodes.Remove(verifyCode);
    //     }

    //     _unitOfWork.TicketOrders.Remove(order);
    //     await _unitOfWork.SaveChangesAsync();
    //     return true;
    // }

    // ... 其他代码 ...

        // ==========================================
        // 酒店退款业务 (User Side)
        // ==========================================

        public async Task<bool> RefundHotelOrderAsync(int orderId, int userId)
        {
            var order = await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId) return false;

            // 只有 "已支付" 或 "待入住" 的订单可以申请退款
            // if (order.Status == HotelOrderStatus.Paid || order.Status == HotelOrderStatus.ToUse)
            // {
            //     order.Status = HotelOrderStatus.RefundRequested; // 变更为申请中
            //     _unitOfWork.HotelOrders.Update(order);
            //     await _unitOfWork.SaveChangesAsync();
            //     return true;
            // }
            // return false;

            order.Status = HotelOrderStatus.Cancelled; // 变更为申请中
                _unitOfWork.HotelOrders.Update(order);
                await _unitOfWork.SaveChangesAsync();
                return true;
        }

        // ==========================================
        // 酒店退款管理 (Admin Side)
        // ==========================================

        // 获取所有酒店订单
        public async Task<List<HotelOrder>> GetAllHotelOrdersAsync()
        {
            var orders = await _unitOfWork.HotelOrders.GetAllAsync(
                o => o.User, 
                o => o.RoomType,
                o => o.RoomType.Hotel // 关联加载酒店信息
            );
            return orders.OrderByDescending(o => o.CreatedAt).ToList();
        }

        // 同意退款 -> 设为 Cancelled
        public async Task<bool> AdminApproveHotelRefundAsync(int orderId)
        {
            var order = await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = HotelOrderStatus.Cancelled;
            _unitOfWork.HotelOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // 拒绝退款 -> 恢复为 ToUse (待入住)
        public async Task<bool> AdminRejectHotelRefundAsync(int orderId)
        {
            var order = await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = HotelOrderStatus.ToUse;
            _unitOfWork.HotelOrders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // // 删除订单 (仅限已取消)
        // public async Task<bool> AdminDeleteHotelOrderAsync(int orderId)
        // {
        //     var order = await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
        //     if (order == null) return false;

        //     if (order.Status == HotelOrderStatus.Cancelled)
        //     {
        //         _unitOfWork.HotelOrders.Remove(order);
        //         await _unitOfWork.SaveChangesAsync();
        //         return true;
        //     }
        //     return false;
        // }


        // // 1. 修复酒店订单删除
        // public async Task<bool> AdminDeleteHotelOrderAsync(int id)
        // {
        //     // 查找订单
        //     var order = await _unitOfWork.HotelOrders.FindAsync(id);
        //     if (order == null) return false;

        //     // =========================================================
        //     // 【核心修复】 先查找并删除关联的核销码 (VerifyCode)
        //     // =========================================================
        //     var verifyCode = await _unitOfWork.VerifyCodes
        //                                 .FirstOrDefaultAsync(v => v.HotelOrderId == id);
        //     if (verifyCode != null)
        //     {
        //         _unitOfWork.VerifyCodes.Remove(verifyCode);
        //     }

        //     // =========================================================
        //     // 【核心修复】 如果有退款记录 (Refunds)，也可能需要删除
        //     // (根据你的报错，目前是VerifyCode卡住了，但如果有Refund表，这里也得删)
        //     // =========================================================
        //     // var refund = await _context.Refunds.FirstOrDefaultAsync(r => r.HotelOrderId == id);
        //     // if (refund != null) _context.Refunds.Remove(refund);

        //     // 最后删除订单
        //     _unitOfWork.HotelOrders.Remove(order);
            
        //     // 提交事务
        //     await _unitOfWork.SaveChangesAsync();
        //     return true;
        // }

        // 【新增】实现同时获取所有订单
       // 【修正】改为串行执行，避免 DbContext 线程冲突
        public async Task<(List<TicketOrder> Tickets, List<HotelOrder> Hotels)> GetAllTicketAndHotelOrdersAsync()
        {
            // 1. 先获取门票订单 (加 await，等待执行完毕)
            var tickets = await _unitOfWork.TicketOrders.GetAllAsync(
                t => t.User, 
                t => t.Attraction
            );

            // 2. 再获取酒店订单 (加 await，等待执行完毕)
            var hotels = await _unitOfWork.HotelOrders.GetAllAsync(
                h => h.User, 
                h => h.RoomType, 
                h => h.RoomType.Hotel
            );

            // 3. 返回结果
            return (
                tickets.OrderByDescending(t => t.CreatedAt).ToList(),
                hotels.OrderByDescending(h => h.CreatedAt).ToList()
            );
        }



                // 【新增】获取所有待核销订单 (Paid, ToUse, CheckedIn)
            public async Task<(List<TicketOrder> Tickets, List<HotelOrder> Hotels)> GetOrdersForVerificationAsync()
            {
                // 1. 获取门票 (状态: Paid 或 ToUse)
                var allTickets = await _unitOfWork.TicketOrders.GetAllAsync(t => t.User, t => t.Attraction, t => t.VerifyCode);
                var pendingTickets = allTickets.Where(t => 
                    t.Status == TicketOrderStatus.Paid || 
                    t.Status == TicketOrderStatus.ToUse
                ).OrderBy(t => t.VisitDate).ToList();

                // 2. 获取酒店 (状态: Paid 或 ToUse 或 CheckedIn)
                var allHotels = await _unitOfWork.HotelOrders.GetAllAsync(h => h.User, h => h.RoomType, h => h.RoomType.Hotel, h => h.VerifyCode);
                var pendingHotels = allHotels.Where(h => 
                    h.Status == HotelOrderStatus.Paid || 
                    h.Status == HotelOrderStatus.ToUse || 
                    h.Status == HotelOrderStatus.CheckedIn
                ).OrderBy(h => h.CheckInDate).ToList();

                return (pendingTickets, pendingHotels);
            }

            // 【新增】核销门票 -> Used
            public async Task<bool> VerifyTicketOrderAsync(int orderId)
            {
                var order = await _unitOfWork.TicketOrders.GetByIdAsync(orderId);
                if (order == null) return false;

                // 只有符合状态的才能核销
                if (order.Status == TicketOrderStatus.Paid || order.Status == TicketOrderStatus.ToUse)
                {
                    order.Status = TicketOrderStatus.Used; // 变更为已使用
                    _unitOfWork.TicketOrders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }
                return false;
            }

            // 【新增】核销酒店 -> Used
            public async Task<bool> VerifyHotelOrderAsync(int orderId)
            {
                var order = await _unitOfWork.HotelOrders.GetByIdAsync(orderId);
                if (order == null) return false;

                // 只有符合状态的才能核销
                if (order.Status == HotelOrderStatus.Paid || 
                    order.Status == HotelOrderStatus.ToUse || 
                    order.Status == HotelOrderStatus.CheckedIn)
                {
                    order.Status = HotelOrderStatus.Used; // 变更为已使用/已离店
                    _unitOfWork.HotelOrders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }
                return false;
            }


                 // =========================================================
        // 🟢
        // =========================================================
        public async Task<bool> AdminDeleteHotelOrderAsync(int id)
        {
            // ❌ 错误写法: await _unitOfWork.HotelOrders.FindAsync(id); 
            // ✅ 正确写法: GetByIdAsync(id)
            var order = await _unitOfWork.HotelOrders.GetByIdAsync(id);
            if (order == null) return false;

            // 1. 先删除关联的核销码
            // 使用 FindAsync + Lambda 表达式查找
            var verifyCodes = await _unitOfWork.VerifyCodes.FindAsync(v => v.HotelOrderId == id);
            var verifyCode = verifyCodes.FirstOrDefault(); // 取出第一个（理论上也就一个）
            
            if (verifyCode != null)
            {
                _unitOfWork.VerifyCodes.Remove(verifyCode);
            }

            // 2. 删除订单
            _unitOfWork.HotelOrders.Remove(order);
            
            // 3. 提交事务
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // =========================================================
        // 🟢 修复 2：AdminDeleteOrderAsync (门票)
        // =========================================================
        public async Task<bool> AdminDeleteOrderAsync(int id)
        {
            // ✅ 使用 GetByIdAsync
            var order = await _unitOfWork.TicketOrders.GetByIdAsync(id);
            if (order == null) return false;

            // 1. 先删除关联的核销码
            var verifyCodes = await _unitOfWork.VerifyCodes.FindAsync(v => v.TicketOrderId == id);
            var verifyCode = verifyCodes.FirstOrDefault();

            if (verifyCode != null)
            {
                _unitOfWork.VerifyCodes.Remove(verifyCode);
            }

            // 2. 删除订单
            _unitOfWork.TicketOrders.Remove(order);
            
            // 3. 提交事务
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        


    }
   
}

