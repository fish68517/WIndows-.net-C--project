using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IVerifyService
    {
        Task<VerifyCode> GenerateCodeAsync(int orderId, string orderType);
        Task<bool> VerifyAsync(string code);
        Task<VerifyCode> GetCodeAsync(string code);
    }
}
