using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly MyDbContext _context;

        public AnnouncementsController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            var announcement = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var announcements = _context.Announcements
                .OrderByDescending(a => a.PublishedAt)
                .ToList();
            
            return View(announcements);
        }
    }
}
