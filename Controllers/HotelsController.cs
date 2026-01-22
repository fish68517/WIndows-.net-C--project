using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class HotelsController : Controller
    {
        private readonly MyDbContext _context;

        public HotelsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: /Hotels
        public async Task<IActionResult> Index(string search)
        {
            // 获取所有酒店查询
            var query = _context.Hotels
                .Include(h => h.District) // 关联加载区域信息
                .AsQueryable();

            // 如果有搜索关键词
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h => h.Name.Contains(search) || h.Address.Contains(search));
            }

            var hotels = await query.ToListAsync();
            return View(hotels);
        }

        // GET: /Hotels/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.District)
                .Include(h => h.RoomTypes) // 关联加载房型
                .Include(h => h.Attraction)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
            {
                return NotFound();
            }

            return View(hotel);
        }
    }
}