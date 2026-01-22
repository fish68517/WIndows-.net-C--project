using TourismPlatform.Models;
using TourismPlatform.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TourismPlatform.Services
{
    public class RecommendService : IRecommendService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecommendService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Attraction>> GetHomeRecommendationsAsync(int userId)
        {
            var allAttractions = await _unitOfWork.Attractions.GetAllAsync();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null)
            {
                // If user not found, return popular attractions
                return allAttractions.OrderByDescending(a => a.ViewCount).Take(6);
            }

            // Get user's favorites to identify preferred categories and districts
            var userFavorites = await _unitOfWork.Favorites.GetAllAsync();
            var userFavoriteAttractions = userFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Attraction)
                .ToList();

            // Get user's orders to identify visited attractions
            var userOrders = await _unitOfWork.TicketOrders.GetAllAsync();
            var visitedAttractionIds = userOrders
                .Where(o => o.UserId == userId && o.Status == TicketOrderStatus.Used)
                .Select(o => o.AttractionId)
                .ToHashSet();

            // Identify preferred categories and districts from favorites
            var preferredCategories = userFavoriteAttractions
                .Select(a => a.CategoryId)
                .Distinct()
                .ToHashSet();

            var preferredDistricts = userFavoriteAttractions
                .Select(a => a.DistrictId)
                .Distinct()
                .ToHashSet();

            // Score attractions based on weighted criteria
            var scored = allAttractions
                .Where(a => !visitedAttractionIds.Contains(a.AttractionId))
                .Select(a => new
                {
                    Attraction = a,
                    Score = CalculateRecommendationScore(a, preferredCategories, preferredDistricts)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Attraction.ViewCount)
                .Take(6)
                .Select(x => x.Attraction);

            return scored;
        }

        public async Task<IEnumerable<Attraction>> GetRelatedAttractionsAsync(int attractionId, int userId)
        {
            var attraction = await _unitOfWork.Attractions.GetByIdAsync(attractionId);
            if (attraction == null)
                return new List<Attraction>();

            var allAttractions = await _unitOfWork.Attractions.GetAllAsync();
            
            // Weighted scoring: same category (3), same district (2), popularity (1)
            var scored = allAttractions
                .Where(a => a.AttractionId != attractionId)
                .Select(a => new
                {
                    Attraction = a,
                    Score = (a.CategoryId == attraction.CategoryId ? 3 : 0) +
                            (a.DistrictId == attraction.DistrictId ? 2 : 0) +
                            (a.ViewCount > 0 ? 1 : 0)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Attraction.ViewCount)
                .Take(6)
                .Select(x => x.Attraction);

            return scored;
        }

        private int CalculateRecommendationScore(Attraction attraction, HashSet<int> preferredCategories, HashSet<int> preferredDistricts)
        {
            int score = 0;

            // Weight 3: Same category as user's favorites
            if (preferredCategories.Contains(attraction.CategoryId))
            {
                score += 3;
            }

            // Weight 2: Same district as user's favorites
            if (preferredDistricts.Contains(attraction.DistrictId))
            {
                score += 2;
            }

            // Weight 1: Popularity (based on view count)
            if (attraction.ViewCount > 0)
            {
                score += 1;
            }

            return score;
        }
    }
}
