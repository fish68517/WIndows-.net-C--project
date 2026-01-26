using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TourismPlatform.Services.Weather;

namespace TourismPlatform.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Index()
        {
            var forecast = await _weatherService.GetJinanForecastAsync();
            return View(forecast);
        }
    }
}