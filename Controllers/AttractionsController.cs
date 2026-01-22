using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class AttractionsController : Controller
    {
        private readonly IAttractionService _attractionService;

        public AttractionsController(IAttractionService attractionService)
        {
            _attractionService = attractionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? districtId, int? categoryId, string searchKeyword)
        {
            IEnumerable<Attraction> attractions;

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                attractions = await _attractionService.SearchAsync(searchKeyword);
            }
            else if (districtId.HasValue)
            {
                attractions = await _attractionService.GetByDistrictAsync(districtId.Value);
            }
            else if (categoryId.HasValue)
            {
                attractions = await _attractionService.GetByCategoryAsync(categoryId.Value);
            }
            else
            {
                attractions = await _attractionService.GetAllAsync();
            }

            // Get districts and categories for filter dropdowns
            var districts = await _attractionService.GetAllDistrictsAsync();
            var categories = await _attractionService.GetAllCategoriesAsync();

            ViewBag.Districts = districts;
            ViewBag.Categories = categories;
            ViewBag.SelectedDistrictId = districtId;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchKeyword = searchKeyword;

            return View(attractions);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var attraction = await _attractionService.GetByIdAsync(id);
            if (attraction == null)
            {
                return NotFound();
            }

            return View(attraction);
        }
    }
}
