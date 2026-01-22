using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string email, string password, string nickname);
        Task<User> LoginAsync(string email, string password);
        Task<bool> UpdateProfileAsync(int userId, string nickname, string bio, IFormFile avatar);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> LogoutAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> SearchUsersAsync(string nickname = null, DateTime? registeredAfter = null, DateTime? registeredBefore = null);
        Task<bool> DisableUserAsync(int userId);
        Task<bool> EnableUserAsync(int userId);
    }
}
