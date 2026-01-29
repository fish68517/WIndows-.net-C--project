using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;
using TourismPlatform.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

// 1. 【核心修复】必须引入这个命名空间，否则 AsNoTracking() 会报错
using Microsoft.EntityFrameworkCore;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminHotelsController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly MyDbContext _context;

        public AdminHotelsController(IHotelService hotelService, MyDbContext context)
        {
            _hotelService = hotelService;
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

            var hotels = await _hotelService.GetAllAsync();

            // ========================= 🔍 调试日志开始 =========================
            try 
            {
                var debugOptions = new JsonSerializerOptions
                {
                    WriteIndented = true, // 格式化输出，方便阅读
                    ReferenceHandler = ReferenceHandler.IgnoreCycles, // 【关键】忽略循环引用，防止报错
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 防止中文变成 \uXXXX
                };

                string jsonString = JsonSerializer.Serialize(hotels, debugOptions);

                Console.WriteLine("\n\n================= 🏨 HOTEL DATA DEBUG START =================\n");
                Console.WriteLine(jsonString);
                Console.WriteLine("\n================= 🏨 HOTEL DATA DEBUG END ===================\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ 序列化日志出错: {ex.Message}\n");
            }
            // ========================= 🔍 调试日志结束 =========================

            return View(hotels);
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
        public async Task<IActionResult> Create(Hotel hotel)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            // ============================================================
            // 【核心修复】移除不需要验证的导航属性和 ImageUrl
            // ============================================================
            ModelState.Remove("District");       // 我们只传 DistrictId
            ModelState.Remove("Attraction");     // 我们只传 AttractionId
            ModelState.Remove("ImageUrl");       // 暂时允许为空（因为还没写上传逻辑）
            ModelState.Remove("HotelRoomTypes"); // 不需要验证
            ModelState.Remove("HotelOrders");    // 不需要验证
            // ============================================================

            if (!ModelState.IsValid)
            {
                // 打印错误日志（调试用）
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Hotel Create验证错误: {error.ErrorMessage}");
                    }
                }

                // 重新加载下拉框数据
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(hotel);
            }

            try
            {
                // 如果 ImageUrl 为空，可以给个默认图，避免数据库报错（视你数据库约束而定）
                if (string.IsNullOrEmpty(hotel.ImageUrl))
                {
                    hotel.ImageUrl = "/images/default-hotel.jpg"; // 或者保持 null
                }

                await _hotelService.CreateAsync(hotel);
                TempData["SuccessMessage"] = "酒店创建成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"创建酒店失败: {ex.Message}");
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(hotel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            var districts = _context.Districts.ToList();
            var attractions = _context.Attractions.ToList();

            ViewBag.Districts = districts;
            ViewBag.Attractions = attractions;

            return View(hotel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hotel hotel)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (id != hotel.HotelId)
            {
                return NotFound();
            }

            // ============================================================
            // 【核心修复】移除不需要验证的导航属性和字段
            // ============================================================
            ModelState.Remove("District");       // 我们只传 DistrictId
            ModelState.Remove("Attraction");     // 我们只传 AttractionId
            ModelState.Remove("ImageUrl");       // 图片单独处理，允许为空
            ModelState.Remove("HotelRoomTypes"); // 子集合不需要验证
            ModelState.Remove("HotelOrders");    // 子集合不需要验证
            // ============================================================

            if (!ModelState.IsValid)
            {
                // 打印错误日志
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Hotel Edit验证错误: {error.ErrorMessage}");
                    }
                }

                // 重新加载下拉框数据
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(hotel);
            }

            try
            {
                // ============================================================
                // 【防止图片丢失】如果用户没有上传新图片，保持原有图片
                // ============================================================
                // 使用 AsNoTracking() 查询，防止与 UpdateAsync 中的对象产生追踪冲突
                // 注意：需要引入 Microsoft.EntityFrameworkCore 命名空间
                var existingHotel = await _context.Hotels
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HotelId == id);
                
                if (existingHotel != null)
                {
                    // 如果表单提交的 ImageUrl 为空（没上传新图），则使用数据库里的旧图路径
                    if (string.IsNullOrEmpty(hotel.ImageUrl))
                    {
                        hotel.ImageUrl = existingHotel.ImageUrl;
                    }
                }
                // ============================================================

                await _hotelService.UpdateAsync(hotel);
                TempData["SuccessMessage"] = "酒店更新成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新酒店失败: {ex.Message}");
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(hotel);
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
                await _hotelService.DeleteAsync(id);
                TempData["SuccessMessage"] = "酒店删除成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除酒店失败: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageRoomTypes(int hotelId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var hotel = await _hotelService.GetByIdAsync(hotelId);
            if (hotel == null)
            {
                return NotFound();
            }

            var roomTypes = _context.HotelRoomTypes
                .Where(rt => rt.HotelId == hotelId)
                .ToList();

            ViewBag.Hotel = hotel;
            return View(roomTypes);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRoomType(int hotelId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var hotel = await _hotelService.GetByIdAsync(hotelId);
            if (hotel == null)
            {
                return NotFound();
            }

            ViewBag.Hotel = hotel;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoomType(int hotelId, HotelRoomType roomType)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var hotel = await _hotelService.GetByIdAsync(hotelId);
            if (hotel == null)
            {
                return NotFound();
            }

            ModelState.Remove("Hotel"); 

            if (!ModelState.IsValid)
            {
                ViewBag.Hotel = hotel;
                return View(roomType);
            }

            try
            {
                roomType.HotelId = hotelId;
                _context.HotelRoomTypes.Add(roomType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "房型创建成功";
                return RedirectToAction("ManageRoomTypes", new { hotelId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"创建房型失败: {ex.Message}");
                ViewBag.Hotel = hotel;
                return View(roomType);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRoomType(int roomTypeId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var roomType = _context.HotelRoomTypes.FirstOrDefault(rt => rt.RoomTypeId == roomTypeId);
            if (roomType == null)
            {
                return NotFound();
            }

            var hotel = await _hotelService.GetByIdAsync(roomType.HotelId);
            ViewBag.Hotel = hotel;

            return View(roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoomType(int roomTypeId, HotelRoomType roomType)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (roomTypeId != roomType.RoomTypeId)
            {
                return NotFound();
            }

            // ============================================================
            // 【核心修复】手动移除 Hotel 导航属性的验证
            // 我们只提交了 HotelId，不需要验证 Hotel 对象是否为空
            // ============================================================
            ModelState.Remove("Hotel"); 
            // 如果还有其他不需要验证的导航属性（比如 Orders），也在这里移除
            // ModelState.Remove("HotelOrders"); 
            // ============================================================

            if (!ModelState.IsValid)
            {
                // 重新加载 ViewBag 数据，防止页面报错
                var hotel = await _hotelService.GetByIdAsync(roomType.HotelId);
                // 如果 Service 没查到，尝试直接用 Context 查（以此为准，确保 ViewBag.Hotel 不为空）
                if (hotel == null)
                {
                     hotel = await _context.Hotels.FindAsync(roomType.HotelId);
                }
                
                ViewBag.Hotel = hotel;
                return View(roomType);
            }

            try
            {
                _context.HotelRoomTypes.Update(roomType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "房型更新成功";
                return RedirectToAction("ManageRoomTypes", new { hotelId = roomType.HotelId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新房型失败: {ex.Message}");
                var hotel = await _hotelService.GetByIdAsync(roomType.HotelId);
                ViewBag.Hotel = hotel;
                return View(roomType);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomType(int roomTypeId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            try
            {
                var roomType = _context.HotelRoomTypes.FirstOrDefault(rt => rt.RoomTypeId == roomTypeId);
                if (roomType == null)
                {
                    return NotFound();
                }

                var hotelId = roomType.HotelId;
                _context.HotelRoomTypes.Remove(roomType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "房型删除成功";
                return RedirectToAction("ManageRoomTypes", new { hotelId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除房型失败: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
