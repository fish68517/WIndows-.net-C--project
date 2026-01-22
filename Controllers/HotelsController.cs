using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class HotelsController : Controller
    {
        private readonly IHotelService _hotelService;

        public HotelsController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<IActionResult> ListByAttraction(int attractionId)
        {
            var hotels = await _hotelService.GetByAttractionAsync(attractionId);
            return PartialView("_HotelList", hotels);
        }
    }
}
