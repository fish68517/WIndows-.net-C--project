using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;
using TourismPlatform.Services;

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

            if (!ModelState.IsValid)
            {
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(hotel);
            }

            try
            {
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

            if (!ModelState.IsValid)
            {
                var districts = _context.Districts.ToList();
                var attractions = _context.Attractions.ToList();
                ViewBag.Districts = districts;
                ViewBag.Attractions = attractions;
                return View(hotel);
            }

            try
            {
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

            if (!ModelState.IsValid)
            {
                var hotel = await _hotelService.GetByIdAsync(roomType.HotelId);
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
