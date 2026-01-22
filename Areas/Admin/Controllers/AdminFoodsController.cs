using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;
using TourismPlatform.Services;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminFoodsController : Controller
    {
        private readonly IFoodService _foodService;
        private readonly MyDbContext _context;

        public AdminFoodsController(IFoodService foodService, MyDbContext context)
        {
            _foodService = foodService;
            _context = context;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminUserId").HasValue;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var foods = await _foodService.GetAllAsync();
            return View(foods);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var districts = _context.Districts.ToList();
            var attractions = _context.Attractions.ToList();

            ViewBag.Districts = districts;
            ViewBag.Attractions = attractions;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food food)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (!ModelState.IsValid)
            {
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(food);
            }

            try
            {
                await _foodService.CreateAsync(food);
                TempData["SuccessMessage"] = "美食创建成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"创建美食失败: {ex.Message}");
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(food);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var food = await _foodService.GetByIdAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            var districts = _context.Districts.ToList();
            var attractions = _context.Attractions.ToList();

            ViewBag.Districts = districts;
            ViewBag.Attractions = attractions;

            return View(food);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Food food)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (id != food.FoodId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(food);
            }

            try
            {
                await _foodService.UpdateAsync(food);
                TempData["SuccessMessage"] = "美食更新成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新美食失败: {ex.Message}");
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(food);
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
                await _foodService.DeleteAsync(id);
                TempData["SuccessMessage"] = "美食删除成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除美食失败: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
