using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismPlatform.Data;
using TourismPlatform.Models;
using TourismPlatform.Repositories;
using TourismPlatform.Services;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminVerifyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyDbContext _context;
        private readonly IVerifyService _verifyService;

        public AdminVerifyController(IUnitOfWork unitOfWork, MyDbContext context, IVerifyService verifyService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _verifyService = verifyService;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminUserId").HasValue;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string orderType = "ticket")
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var verifyCodes = await _context.VerifyCodes
                .Where(v => v.Status == VerifyCodeStatus.Unused)
                .AsQueryable()
                .ToListAsync();

            var model = new AdminVerifyListViewModel
            {
                OrderType = orderType,
                VerifyCodes = new List<AdminVerifyCodeViewModel>()
            };

            foreach (var code in verifyCodes)
            {
                if (orderType == "ticket" && code.TicketOrderId.HasValue)
                {
                    var order = await _context.TicketOrders
                        .Include(o => o.User)
                        .Include(o => o.Attraction)
                        .FirstOrDefaultAsync(o => o.TicketOrderId == code.TicketOrderId);

                    if (order != null)
                    {
                        model.VerifyCodes.Add(new AdminVerifyCodeViewModel
                        {
                            VerifyCodeId = code.VerifyCodeId,
                            Code = code.Code,
                            OrderId = code.TicketOrderId.Value,
                            OrderType = "Ticket",
                            UserNickname = order.User?.Nickname ?? "未知用户",
                            UserEmail = order.User?.Email ?? "未知用户",
                            ItemName = order.Attraction?.Name ?? "未知景点",
                            OrderDate = order.CreatedAt,
                            Status = code.Status.ToString(),
                            CreatedAt = code.CreatedAt
                        });
                    }
                }
                else if (orderType == "hotel" && code.HotelOrderId.HasValue)
                {
                    var order = await _context.HotelOrders
                        .Include(o => o.User)
                        .Include(o => o.RoomType)
                        .ThenInclude(rt => rt.Hotel)
                        .FirstOrDefaultAsync(o => o.HotelOrderId == code.HotelOrderId);

                    if (order != null)
                    {
                        model.VerifyCodes.Add(new AdminVerifyCodeViewModel
                        {
                            VerifyCodeId = code.VerifyCodeId,
                            Code = code.Code,
                            OrderId = code.HotelOrderId.Value,
                            OrderType = "Hotel",
                            UserNickname = order.User?.Nickname ?? "未知用户",
                            UserEmail = order.User?.Email ?? "未知用户",
                            ItemName = order.RoomType?.Hotel?.Name ?? "未知酒店",
                            OrderDate = order.CreatedAt,
                            Status = code.Status.ToString(),
                            CreatedAt = code.CreatedAt
                        });
                    }
                }
            }

            model.VerifyCodes = model.VerifyCodes.OrderByDescending(v => v.CreatedAt).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string code)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "未授权" });
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "核销码不能为空" });
            }

            try
            {
                var verifyCode = await _verifyService.GetCodeAsync(code.Trim());
                
                if (verifyCode == null)
                {
                    return Json(new { success = false, message = "核销码不存在" });
                }

                if (verifyCode.Status == VerifyCodeStatus.Used)
                {
                    return Json(new { success = false, message = "核销码已被使用" });
                }

                // Verify the code
                var result = await _verifyService.VerifyAsync(code.Trim());
                
                if (result)
                {
                    return Json(new { success = true, message = "核销成功" });
                }
                else
                {
                    return Json(new { success = false, message = "核销失败，请重试" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"核销失败: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var verifyCode = await _context.VerifyCodes
                .FirstOrDefaultAsync(v => v.VerifyCodeId == id);

            if (verifyCode == null)
            {
                return NotFound("核销码不存在");
            }

            AdminVerifyDetailViewModel model = null;

            if (verifyCode.TicketOrderId.HasValue)
            {
                var order = await _context.TicketOrders
                    .Include(o => o.User)
                    .Include(o => o.Attraction)
                    .FirstOrDefaultAsync(o => o.TicketOrderId == verifyCode.TicketOrderId);

                if (order != null)
                {
                    model = new AdminVerifyDetailViewModel
                    {
                        VerifyCodeId = verifyCode.VerifyCodeId,
                        Code = verifyCode.Code,
                        OrderType = "Ticket",
                        UserNickname = order.User?.Nickname ?? "未知用户",
                        UserEmail = order.User?.Email ?? "未知用户",
                        ItemName = order.Attraction?.Name ?? "未知景点",
                        VisitDate = order.VisitDate,
                        Quantity = order.Quantity,
                        TotalPrice = order.TotalPrice,
                        OrderStatus = order.Status.ToString(),
                        VerifyStatus = verifyCode.Status.ToString(),
                        CreatedAt = verifyCode.CreatedAt,
                        UsedAt = verifyCode.UsedAt
                    };
                }
            }
            else if (verifyCode.HotelOrderId.HasValue)
            {
                var order = await _context.HotelOrders
                    .Include(o => o.User)
                    .Include(o => o.RoomType)
                    .ThenInclude(rt => rt.Hotel)
                    .FirstOrDefaultAsync(o => o.HotelOrderId == verifyCode.HotelOrderId);

                if (order != null)
                {
                    model = new AdminVerifyDetailViewModel
                    {
                        VerifyCodeId = verifyCode.VerifyCodeId,
                        Code = verifyCode.Code,
                        OrderType = "Hotel",
                        UserNickname = order.User?.Nickname ?? "未知用户",
                        UserEmail = order.User?.Email ?? "未知用户",
                        ItemName = order.RoomType?.Hotel?.Name ?? "未知酒店",
                        CheckInDate = order.CheckInDate,
                        CheckOutDate = order.CheckOutDate,
                        TotalPrice = order.TotalPrice,
                        OrderStatus = order.Status.ToString(),
                        VerifyStatus = verifyCode.Status.ToString(),
                        CreatedAt = verifyCode.CreatedAt,
                        UsedAt = verifyCode.UsedAt
                    };
                }
            }

            if (model == null)
            {
                return NotFound("订单信息不存在");
            }

            return View(model);
        }
    }

  
}
