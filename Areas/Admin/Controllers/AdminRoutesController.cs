using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminRoutesController : Controller
    {
        private readonly MyDbContext _context;

        public AdminRoutesController(MyDbContext context)
        {
            _context = context;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminUserId").HasValue;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var routes = _context.TravelRoutes.ToList();
            return View(routes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TravelRoute route)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (!ModelState.IsValid)
            {
                return View(route);
            }

            try
            {
                route.CreatedAt = DateTime.UtcNow;
                route.UpdatedAt = DateTime.UtcNow;
                _context.TravelRoutes.Add(route);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "线路创建成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"创建线路失败: {ex.Message}");
                return View(route);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var route = _context.TravelRoutes.FirstOrDefault(r => r.RouteId == id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TravelRoute route)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (id != route.RouteId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(route);
            }

            try
            {
                var existingRoute = _context.TravelRoutes.FirstOrDefault(r => r.RouteId == id);
                if (existingRoute == null)
                {
                    return NotFound();
                }

                existingRoute.Name = route.Name;
                existingRoute.Description = route.Description;
                existingRoute.Itinerary = route.Itinerary;
                existingRoute.AttractionIntroduction = route.AttractionIntroduction;
                existingRoute.TransportSuggestion = route.TransportSuggestion;
                existingRoute.UpdatedAt = DateTime.UtcNow;

                _context.TravelRoutes.Update(existingRoute);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "线路更新成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新线路失败: {ex.Message}");
                return View(route);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            try
            {
                var route = _context.TravelRoutes.FirstOrDefault(r => r.RouteId == id);
                if (route == null)
                {
                    return NotFound();
                }

                _context.TravelRoutes.Remove(route);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "线路删除成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除线路失败: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
