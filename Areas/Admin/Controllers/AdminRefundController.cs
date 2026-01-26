using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TourismPlatform.Services;
using Microsoft.AspNetCore.Http;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{


    // 1. 【核心修复】添加这个特性，告诉系统这个控制器属于 Admin 区域
    [Area("Admin")]
    public class AdminRefundController : Controller
    {
        private readonly IOrderService _orderService;

        public AdminRefundController(IOrderService orderService)
        {
            _orderService = orderService;
        }

       // 列表页
        public async Task<IActionResult> Index()
        {
            // 【修改】使用新方法一次性获取所有数据
            var (tickets, hotels) = await _orderService.GetAllTicketAndHotelOrdersAsync();

            var model = new RefundManagementViewModel
            {
                TicketOrders = tickets,
                HotelOrders = hotels
            };

            return View(model);
        }

        // 同意退票
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            await _orderService.AdminApproveRefundAsync(id);
            TempData["SuccessMessage"] = "已同意退票，订单取消。";
            return RedirectToAction(nameof(Index));
        }

        // 拒绝退票
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            await _orderService.AdminRejectRefundAsync(id);
            TempData["SuccessMessage"] = "已拒绝退票，订单恢复使用。";
            return RedirectToAction(nameof(Index));
        }

        // 删除订单
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderService.AdminDeleteOrderAsync(id);
            if (success)
                TempData["SuccessMessage"] = "订单已彻底删除。";
            else
                TempData["ErrorMessage"] = "删除失败，只有已取消的订单可以删除。";
                
            return RedirectToAction(nameof(Index));
        }

                [HttpPost]
            public async Task<IActionResult> ApproveHotel(int id)
            {
                await _orderService.AdminApproveHotelRefundAsync(id);
                TempData["SuccessMessage"] = "酒店退款已批准。";
                return RedirectToAction(nameof(Index));
            }

            [HttpPost]
            public async Task<IActionResult> RejectHotel(int id)
            {
                await _orderService.AdminRejectHotelRefundAsync(id);
                TempData["SuccessMessage"] = "酒店退款已拒绝。";
                return RedirectToAction(nameof(Index));
            }

            [HttpPost]
            public async Task<IActionResult> DeleteHotel(int id)
            {
                await _orderService.AdminDeleteHotelOrderAsync(id);
                TempData["SuccessMessage"] = "酒店订单已删除。";
                return RedirectToAction(nameof(Index));
            }
    }
}