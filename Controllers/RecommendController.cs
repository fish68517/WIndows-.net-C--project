using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class RecommendController : Controller
    {
        private readonly IRecommendService _recommendService;

        public RecommendController(IRecommendService recommendService)
        {
            _recommendService = recommendService;
        }

        public async Task<IActionResult> ForHome()
        {
            var userIdObj = HttpContext.Session.GetInt32("UserId");
            
            if (userIdObj.HasValue)
            {
                var recommendations = await _recommendService.GetHomeRecommendationsAsync(userIdObj.Value);
                return PartialView("_Recommendations", recommendations);
            }
            else
            {
                // Return empty recommendations for anonymous users
                return PartialView("_Recommendations", new List<Models.Attraction>());
            }
        }

        public async Task<IActionResult> ForAttraction(int attractionId)
        {
            var userIdObj = HttpContext.Session.GetInt32("UserId");
            int userId = userIdObj ?? 0;
            
            var recommendations = await _recommendService.GetRelatedAttractionsAsync(attractionId, userId);
            return PartialView("_Recommendations", recommendations);
        }
    }
}
