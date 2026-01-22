using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IFavoriteService
    {
        Task<bool> AddAsync(int userId, int attractionId);
        Task<bool> RemoveAsync(int userId, int attractionId);
        Task<IEnumerable<Attraction>> GetUserFavoritesAsync(int userId);
        Task<bool> IsFavoritedAsync(int userId, int attractionId);
    }
}
