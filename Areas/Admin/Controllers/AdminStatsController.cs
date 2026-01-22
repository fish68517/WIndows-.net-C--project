using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminStatsController : Controller
    {
        private readonly IStatsService _statsService;

        public AdminStatsController(IStatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if admin is logged in
            if (!HttpContext.Session.GetInt32("AdminUserId").HasValue)
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var stats = await _statsService.GetStatsAsync();
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatsData()
        {
            // Check if admin is logged in
            if (!HttpContext.Session.GetInt32("AdminUserId").HasValue)
            {
                return Unauthorized();
            }

            var stats = await _statsService.GetStatsAsync();
            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopAttractionsChartData()
        {
            // Check if admin is logged in
            if (!HttpContext.Session.GetInt32("AdminUserId").HasValue)
            {
                return Unauthorized();
            }

            var stats = await _statsService.GetStatsAsync();
            
            var chartData = new
            {
                labels = stats.TopAttractions.Select(a => a.Name).ToList(),
                datasets = new[]
                {
                    new
                    {
                        label = "浏览次数",
                        data = stats.TopAttractions.Select(a => a.ViewCount).ToList(),
                        backgroundColor = "rgba(54, 162, 235, 0.5)",
                        borderColor = "rgba(54, 162, 235, 1)",
                        borderWidth = 1
                    },
                    new
                    {
                        label = "收藏数",
                        data = stats.TopAttractions.Select(a => a.FavoriteCount).ToList(),
                        backgroundColor = "rgba(75, 192, 192, 0.5)",
                        borderColor = "rgba(75, 192, 192, 1)",
                        borderWidth = 1
                    },
                    new
                    {
                        label = "订单数",
                        data = stats.TopAttractions.Select(a => a.OrderCount).ToList(),
                        backgroundColor = "rgba(255, 159, 64, 0.5)",
                        borderColor = "rgba(255, 159, 64, 1)",
                        borderWidth = 1
                    }
                }
            };

            return Json(chartData);
        }
    }
}
