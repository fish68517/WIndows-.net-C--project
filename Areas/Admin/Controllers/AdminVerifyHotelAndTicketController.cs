using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TourismPlatform.Services;
using TourismPlatform.Models; // 确保引用了 Models

namespace TourismPlatform.Controllers
{
    [Area("Admin")] // 确保有 Area 标记
    public class AdminVerifyHotelAndTicketController : Controller
    {
        private readonly IOrderService _orderService;

        public AdminVerifyHotelAndTicketController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 列表页：展示待核销订单
        public async Task<IActionResult> Index()
        {
            // 打印日志
            
            var (tickets, hotels) = await _orderService.GetOrdersForVerificationAsync();

            // 【关键修改】这里必须实例化 VerificationViewModel
            var model = new VerificationViewModel
            {
                TicketOrders = tickets,
                HotelOrders = hotels
            };

            // 传给 View 的 model 类型必须是 VerificationViewModel
            return View(model);
        }

        // 核销门票
        [HttpPost]
        public async Task<IActionResult> VerifyTicket(int id)
        {
            var success = await _orderService.VerifyTicketOrderAsync(id);
            if (success)
                TempData["SuccessMessage"] = "门票核销成功！";
            else
                TempData["ErrorMessage"] = "核销失败，订单状态可能已变更。";
                
            return RedirectToAction(nameof(Index));
        }

        // 核销酒店
        [HttpPost]
        public async Task<IActionResult> VerifyHotel(int id)
        {
            var success = await _orderService.VerifyHotelOrderAsync(id);
            if (success)
                TempData["SuccessMessage"] = "酒店订单核销成功！";
            else
                TempData["ErrorMessage"] = "核销失败，订单状态可能已变更。";

            return RedirectToAction(nameof(Index));
        }
    }
}