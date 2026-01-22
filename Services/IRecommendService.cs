using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IRecommendService
    {
        Task<IEnumerable<Attraction>> GetHomeRecommendationsAsync(int userId);
        Task<IEnumerable<Attraction>> GetRelatedAttractionsAsync(int attractionId, int userId);
    }
}
