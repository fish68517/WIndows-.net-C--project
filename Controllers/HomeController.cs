using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDbContext _context;

        public HomeController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var latestAnnouncements = _context.Announcements
                .OrderByDescending(a => a.PublishedAt)
                .Take(3)
                .ToList();
            
            ViewData["Announcements"] = latestAnnouncements;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
