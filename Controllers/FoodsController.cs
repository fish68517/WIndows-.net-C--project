using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class FoodsController : Controller
    {
        private readonly IFoodService _foodService;

        public FoodsController(IFoodService foodService)
        {
            _foodService = foodService;
        }

        [HttpGet]
        public async Task<IActionResult> ListByAttraction(int attractionId)
        {
            var foods = await _foodService.GetByAttractionAsync(attractionId);
            return PartialView("_FoodList", foods);
        }
    }
}
