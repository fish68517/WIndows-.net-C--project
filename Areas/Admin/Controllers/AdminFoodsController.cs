using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;
using TourismPlatform.Services;
using Microsoft.EntityFrameworkCore; // 1. 必须引入这个，否则 AsNoTracking 会报错

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

// 1. 【核心修复】必须引入这个命名空间，否则 AsNoTracking() 会报错
using Microsoft.EntityFrameworkCore;

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

            // ========================= 🔍 调试代码开始 =========================
            try
            {
                var debugOptions = new JsonSerializerOptions
                {
                    WriteIndented = true, // 格式化输出，方便阅读
                    ReferenceHandler = ReferenceHandler.IgnoreCycles, // 防止循环引用报错
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 防止中文被转码
                };

                string jsonString = JsonSerializer.Serialize(foods, debugOptions);

                Console.WriteLine("\n\n================= 🍲 FOOD DATA DEBUG START =================\n");
                Console.WriteLine(jsonString);
                Console.WriteLine("\n================= 🍲 FOOD DATA DEBUG END ===================\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ JSON 调试打印出错: {ex.Message}\n");
            }
            // ========================= 🔍 调试代码结束 =========================

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

            // ============================================================
            // 【核心修复】移除不需要验证的导航属性
            // ============================================================
            ModelState.Remove("District");     // 只传 DistrictId
            ModelState.Remove("Attraction");   // 只传 AttractionId
           // ModelState.Remove("ImageUrl");     // 允许为空 (还没做上传功能前)
            // ============================================================

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
                // 给个默认图防止数据库报错 (如果 ImageUrl 必填)
                // if (string.IsNullOrEmpty(food.ImageUrl))
                // {
                //     food.ImageUrl = "/images/default-food.jpg";
                // }

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

            // ============================================================
            // 【核心修复】移除导航属性验证
            // ============================================================
            ModelState.Remove("District");
            ModelState.Remove("Attraction");
            // ModelState.Remove("ImageUrl");
            // ============================================================

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
                // ============================================================
                // 【防止图片丢失】如果没传新图，保持旧图
                // ============================================================
                // 假设 DbContext 中有名为 Foods 的 DbSet
                var existingFood = await _context.Foods
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FoodId == id);
                
                if (existingFood != null)
                {
                    // if (string.IsNullOrEmpty(food.ImageUrl))
                    // {
                    //     food.ImageUrl = existingFood.ImageUrl;
                    // }
                }
                // ============================================================

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