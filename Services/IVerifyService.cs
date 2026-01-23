using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IVerifyService
    {
        // 修改：返回 VerifyCode 对象，而不是 void (Task)
        Task<VerifyCode> GenerateCodeAsync(int orderId, string type);
        Task<bool> VerifyAsync(string code);
        Task<VerifyCode> GetCodeAsync(string code);
    }
}
