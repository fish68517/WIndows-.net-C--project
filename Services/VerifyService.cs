using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class VerifyService : IVerifyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VerifyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VerifyCode> GenerateCodeAsync(int orderId, string orderType)
        {
            var code = GenerateUniqueCode();
            
            var verifyCode = new VerifyCode
            {
                Code = code,
                TicketOrderId = orderType == "Ticket" ? orderId : null,
                HotelOrderId = orderType == "Hotel" ? orderId : null,
                OrderType = orderType == "Ticket" ? OrderType.Ticket : OrderType.Hotel,
                Status = VerifyCodeStatus.Unused,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.VerifyCodes.AddAsync(verifyCode);
            await _unitOfWork.SaveChangesAsync();
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
