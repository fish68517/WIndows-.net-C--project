using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FavoriteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddAsync(int userId, int attractionId)
        {
            var exists = await _unitOfWork.Favorites.AnyAsync(f => f.UserId == userId && f.AttractionId == attractionId);
            if (exists)
                return false;

            var favorite = new Favorite
            {
                UserId = userId,
                AttractionId = attractionId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Favorites.AddAsync(favorite);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAsync(int userId, int attractionId)
        {
            var favorite = await _unitOfWork.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.AttractionId == attractionId);
            if (favorite == null)
                return false;

            _unitOfWork.Favorites.Remove(favorite);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Attraction>> GetUserFavoritesAsync(int userId)
        {
            var favorites = await _unitOfWork.Favorites.FindAsync(f => f.UserId == userId);
            var attractionIds = favorites.Select(f => f.AttractionId).ToList();
            
            var attractions = new List<Attraction>();
            foreach (var id in attractionIds)
            {
                var attraction = await _unitOfWork.Attractions.GetByIdAsync(id);
                if (attraction != null)
                    attractions.Add(attraction);
            }
            
            return attractions;
        }

        public async Task<bool> IsFavoritedAsync(int userId, int attractionId)
        {
            return await _unitOfWork.Favorites.AnyAsync(f => f.UserId == userId && f.AttractionId == attractionId);
        }
    }
}
