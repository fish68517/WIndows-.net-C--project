using TourismPlatform.Models;
using TourismPlatform.Repositories;

using Microsoft.EntityFrameworkCore; // 引入此命名空间以使用 AnyAsync 或 FirstOrDefaultAsync
using System;
using System.Linq; // 引入 Linq
using System.Threading.Tasks;

namespace TourismPlatform.Services
{
    public class VerifyService : IVerifyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VerifyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 修改：返回类型改为 Task<VerifyCode>
        public async Task<VerifyCode> GenerateCodeAsync(int orderId, string type)
        {
            // 1. 检查是否已经生成过 (使用 FirstOrDefault)
            var existingCodes = await _unitOfWork.VerifyCodes.FindAsync(v => 
                (type == "Ticket" && v.TicketOrderId == orderId) || 
                (type == "Hotel" && v.HotelOrderId == orderId));

            var existingCode = existingCodes.FirstOrDefault();
            if (existingCode != null) 
            {
                return existingCode; 
            }

            // 2. 生成唯一的核销码字符串
            string prefix = type == "Ticket" ? "T" : "H";
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            string codeStr = $"{prefix}-{datePart}-{randomPart}";

            // 3. 创建实体对象
            var verifyCode = new VerifyCode
            {
                Code = codeStr,
                // ==========================================
                // 修复点：添加 (OrderType) 强制转换
                // ==========================================
                OrderType = (OrderType)(type == "Ticket" ? 1 : 2), 
                
                Status = 0, // 0=未使用
                CreatedAt = DateTime.UtcNow
            };

            if (type == "Ticket") verifyCode.TicketOrderId = orderId;
            else verifyCode.HotelOrderId = orderId;

            // 4. 保存到数据库
            await _unitOfWork.VerifyCodes.AddAsync(verifyCode);
            await _unitOfWork.SaveChangesAsync();

            // 5. 返回对象
            return verifyCode;
        }

        public async Task<bool> VerifyAsync(string code)
        {
            var verifyCode = await _unitOfWork.VerifyCodes.FirstOrDefaultAsync(v => v.Code == code);
            if (verifyCode == null || verifyCode.Status == VerifyCodeStatus.Used)
                return false;

            verifyCode.Status = VerifyCodeStatus.Used;
            verifyCode.UsedAt = DateTime.UtcNow;
            _unitOfWork.VerifyCodes.Update(verifyCode);
            await _unitOfWork.SaveChangesAsync();

            // Update order status to "Used"
            if (verifyCode.TicketOrderId.HasValue)
            {
                var order = await _unitOfWork.TicketOrders.GetByIdAsync(verifyCode.TicketOrderId.Value);
                if (order != null)
                {
                    order.Status = TicketOrderStatus.Used;
                    _unitOfWork.TicketOrders.Update(order);
                }
            }
            else if (verifyCode.HotelOrderId.HasValue)
            {
                var order = await _unitOfWork.HotelOrders.GetByIdAsync(verifyCode.HotelOrderId.Value);
                if (order != null)
                {
                    order.Status = HotelOrderStatus.Used;
                    _unitOfWork.HotelOrders.Update(order);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<VerifyCode> GetCodeAsync(string code)
        {
            return await _unitOfWork.VerifyCodes.FirstOrDefaultAsync(v => v.Code == code);
        }

        private string GenerateUniqueCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        }
    }
}
