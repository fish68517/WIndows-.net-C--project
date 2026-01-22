using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminHomeController : Controller
    {
        private readonly MyDbContext _context;

        public AdminHomeController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Check if admin is logged in
            if (!HttpContext.Session.GetInt32("AdminUserId").HasValue)
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            // Calculate statistics
            var stats = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(),
                TotalAttractions = _context.Attractions.Count(),
                TotalDiaries = _context.TravelDiaries.Count(),
                TotalTicketOrders = _context.TicketOrders.Count(),
                TotalHotelOrders = _context.HotelOrders.Count(),
                TotalRevenue = _context.TicketOrders
                    .Where(o => o.Status == TicketOrderStatus.Paid || o.Status == TicketOrderStatus.Used)
                    .Sum(o => o.TotalPrice) +
                    _context.HotelOrders
                    .Where(o => o.Status == HotelOrderStatus.Paid || o.Status == HotelOrderStatus.Used || o.Status == HotelOrderStatus.CheckedIn)
                    .Sum(o => o.TotalPrice),
                PendingTicketOrders = _context.TicketOrders
                    .Where(o => o.Status == TicketOrderStatus.PendingPay)
                    .Count(),
                PendingHotelOrders = _context.HotelOrders
                    .Where(o => o.Status == HotelOrderStatus.PendingPay)
                    .Count(),
                AdminUsername = HttpContext.Session.GetString("AdminUsername")
            };

            return View(stats);
        }
    }

 
}
