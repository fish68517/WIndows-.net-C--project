using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismPlatform.Data;
using TourismPlatform.Models;
using TourismPlatform.Repositories;
using OfficeOpenXml;
using TourismPlatform.Models;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyDbContext _context;

        public AdminOrdersController(IUnitOfWork unitOfWork, MyDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminUserId").HasValue;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string orderType = "ticket", string status = "")
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (orderType == "hotel")
            {
                return await GetHotelOrders(status);
            }
            else
            {
                return await GetTicketOrders(status);
            }
        }

        private async Task<IActionResult> GetTicketOrders(string status)
        {
            var orders = _context.TicketOrders
                .Include(o => o.User)
                .Include(o => o.Attraction)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<TicketOrderStatus>(status, out var statusEnum))
                {
                    orders = orders.Where(o => o.Status == statusEnum);
                }
            }

            var orderList = orders.OrderByDescending(o => o.CreatedAt).ToList();

            var model = new AdminOrderListViewModel
            {
                OrderType = "ticket",
                SelectedStatus = status,
                TicketOrders = orderList.Select(o => new AdminTicketOrderViewModel
                {
                    TicketOrderId = o.TicketOrderId,
                    UserEmail = o.User?.Email ?? "未知用户",
                    UserNickname = o.User?.Nickname ?? "未知用户",
                    AttractionName = o.Attraction?.Name ?? "未知景点",
                    VisitDate = o.VisitDate,
                    Quantity = o.Quantity,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    PaidAt = o.PaidAt
                }).ToList()
            };

            return View("Index", model);
        }

        private async Task<IActionResult> GetHotelOrders(string status)
        {
            var orders = _context.HotelOrders
                .Include(o => o.User)
                .Include(o => o.RoomType)
                .ThenInclude(rt => rt.Hotel)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<HotelOrderStatus>(status, out var statusEnum))
                {
                    orders = orders.Where(o => o.Status == statusEnum);
                }
            }

            var orderList = orders.OrderByDescending(o => o.CreatedAt).ToList();

            var model = new AdminOrderListViewModel
            {
                OrderType = "hotel",
                SelectedStatus = status,
                HotelOrders = orderList.Select(o => new AdminHotelOrderViewModel
                {
                    HotelOrderId = o.HotelOrderId,
                    UserEmail = o.User?.Email ?? "未知用户",
                    UserNickname = o.User?.Nickname ?? "未知用户",
                    HotelName = o.RoomType?.Hotel?.Name ?? "未知酒店",
                    RoomTypeName = o.RoomType?.RoomTypeName ?? "未知房型",
                    CheckInDate = o.CheckInDate,
                    CheckOutDate = o.CheckOutDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    PaidAt = o.PaidAt
                }).ToList()
            };

            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> DetailTicket(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var order = _context.TicketOrders
                .Include(o => o.User)
                .Include(o => o.Attraction)
                .Include(o => o.VerifyCode)
                .FirstOrDefault(o => o.TicketOrderId == id);

            if (order == null)
            {
                return NotFound("订单不存在");
            }

            var model = new AdminTicketOrderDetailViewModel
            {
                TicketOrderId = order.TicketOrderId,
                UserEmail = order.User?.Email ?? "未知用户",
                UserNickname = order.User?.Nickname ?? "未知用户",
                AttractionName = order.Attraction?.Name ?? "未知景点",
                VisitDate = order.VisitDate,
                Quantity = order.Quantity,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                VerifyCode = order.VerifyCode?.Code ?? "未生成"
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DetailHotel(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var order = _context.HotelOrders
                .Include(o => o.User)
                .Include(o => o.RoomType)
                .ThenInclude(rt => rt.Hotel)
                .Include(o => o.VerifyCode)
                .FirstOrDefault(o => o.HotelOrderId == id);

            if (order == null)
            {
                return NotFound("订单不存在");
            }

            var model = new AdminHotelOrderDetailViewModel
            {
                HotelOrderId = order.HotelOrderId,
                UserEmail = order.User?.Email ?? "未知用户",
                UserNickname = order.User?.Nickname ?? "未知用户",
                HotelName = order.RoomType?.Hotel?.Name ?? "未知酒店",
                RoomTypeName = order.RoomType?.RoomTypeName ?? "未知房型",
                CheckInDate = order.CheckInDate,
                CheckOutDate = order.CheckOutDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                VerifyCode = order.VerifyCode?.Code ?? "未生成"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTicketStatus(int id, string newStatus)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "未授权" });
            }

            try
            {
                var order = await _unitOfWork.TicketOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "订单不存在" });
                }

                if (Enum.TryParse<TicketOrderStatus>(newStatus, out var statusEnum))
                {
                    order.Status = statusEnum;
                    _unitOfWork.TicketOrders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                    return Json(new { success = true, message = "订单状态已更新" });
                }

                return Json(new { success = false, message = "无效的状态值" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失败: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHotelStatus(int id, string newStatus)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "未授权" });
            }

            try
            {
                var order = await _unitOfWork.HotelOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "订单不存在" });
                }

                if (Enum.TryParse<HotelOrderStatus>(newStatus, out var statusEnum))
                {
                    order.Status = statusEnum;
                    _unitOfWork.HotelOrders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                    return Json(new { success = true, message = "订单状态已更新" });
                }

                return Json(new { success = false, message = "无效的状态值" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失败: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTicketOrder(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "未授权" });
            }

            try
            {
                var order = await _unitOfWork.TicketOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "订单不存在" });
                }

                if (order.Status == TicketOrderStatus.Cancelled)
                {
                    return Json(new { success = false, message = "订单已取消" });
                }

                order.Status = TicketOrderStatus.Cancelled;
                _unitOfWork.TicketOrders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "订单已取消，退款已处理" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"取消失败: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelHotelOrder(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "未授权" });
            }

            try
            {
                var order = await _unitOfWork.HotelOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "订单不存在" });
                }

                if (order.Status == HotelOrderStatus.Cancelled)
                {
                    return Json(new { success = false, message = "订单已取消" });
                }

                order.Status = HotelOrderStatus.Cancelled;
                _unitOfWork.HotelOrders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "订单已取消，退款已处理" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"取消失败: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string orderType = "ticket", string startDate = "", string endDate = "", string attractionId = "", string hotelId = "")
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            try
            {
               // EPPlus.LicenseContext.LicenseType = EPPlus.LicenseType.Community;
                // 确保文件头部有 using OfficeOpenXml;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    if (orderType == "hotel")
                    {
                        await ExportHotelOrders(package, startDate, endDate, hotelId);
                    }
                    else
                    {
                        await ExportTicketOrders(package, startDate, endDate, attractionId);
                    }

                    var fileName = $"Orders_{orderType}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"导出失败: {ex.Message}");
            }
        }

        private async Task ExportTicketOrders(ExcelPackage package, string startDate, string endDate, string attractionId)
        {
            var worksheet = package.Workbook.Worksheets.Add("门票订单");

            // Add headers
            worksheet.Cells[1, 1].Value = "订单号";
            worksheet.Cells[1, 2].Value = "用户邮箱";
            worksheet.Cells[1, 3].Value = "用户昵称";
            worksheet.Cells[1, 4].Value = "景点名称";
            worksheet.Cells[1, 5].Value = "访问日期";
            worksheet.Cells[1, 6].Value = "数量";
            worksheet.Cells[1, 7].Value = "总价";
            worksheet.Cells[1, 8].Value = "订单状态";
            worksheet.Cells[1, 9].Value = "创建时间";
            worksheet.Cells[1, 10].Value = "支付时间";

            // Style headers
            for (int col = 1; col <= 10; col++)
            {
                worksheet.Cells[1, col].Style.Font.Bold = true;
                worksheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Query orders
            var query = _context.TicketOrders
                .Include(o => o.User)
                .Include(o => o.Attraction)
                .AsQueryable();

            // Filter by date range
            if (DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(o => o.CreatedAt >= start);
            }

            if (DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(o => o.CreatedAt <= end.AddDays(1));
            }

            // Filter by attraction
            if (int.TryParse(attractionId, out var attId) && attId > 0)
            {
                query = query.Where(o => o.AttractionId == attId);
            }

            var orders = query.OrderByDescending(o => o.CreatedAt).ToList();

            // Add data rows
            int row = 2;
            foreach (var order in orders)
            {
                worksheet.Cells[row, 1].Value = order.TicketOrderId;
                worksheet.Cells[row, 2].Value = order.User?.Email ?? "未知用户";
                worksheet.Cells[row, 3].Value = order.User?.Nickname ?? "未知用户";
                worksheet.Cells[row, 4].Value = order.Attraction?.Name ?? "未知景点";
                worksheet.Cells[row, 5].Value = order.VisitDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 6].Value = order.Quantity;
                worksheet.Cells[row, 7].Value = order.TotalPrice;
                worksheet.Cells[row, 8].Value = order.Status.ToString();
                worksheet.Cells[row, 9].Value = order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 10].Value = order.PaidAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
        }

        private async Task ExportHotelOrders(ExcelPackage package, string startDate, string endDate, string hotelId)
        {
            var worksheet = package.Workbook.Worksheets.Add("酒店订单");

            // Add headers
            worksheet.Cells[1, 1].Value = "订单号";
            worksheet.Cells[1, 2].Value = "用户邮箱";
            worksheet.Cells[1, 3].Value = "用户昵称";
            worksheet.Cells[1, 4].Value = "酒店名称";
            worksheet.Cells[1, 5].Value = "房型";
            worksheet.Cells[1, 6].Value = "入住日期";
            worksheet.Cells[1, 7].Value = "离店日期";
            worksheet.Cells[1, 8].Value = "总价";
            worksheet.Cells[1, 9].Value = "订单状态";
            worksheet.Cells[1, 10].Value = "创建时间";
            worksheet.Cells[1, 11].Value = "支付时间";

            // Style headers
            for (int col = 1; col <= 11; col++)
            {
                worksheet.Cells[1, col].Style.Font.Bold = true;
                worksheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Query orders
            var query = _context.HotelOrders
                .Include(o => o.User)
                .Include(o => o.RoomType)
                .ThenInclude(rt => rt.Hotel)
                .AsQueryable();

            // Filter by date range
            if (DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(o => o.CreatedAt >= start);
            }

            if (DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(o => o.CreatedAt <= end.AddDays(1));
            }

            // Filter by hotel
            if (int.TryParse(hotelId, out var hId) && hId > 0)
            {
                query = query.Where(o => o.RoomType.HotelId == hId);
            }

            var orders = query.OrderByDescending(o => o.CreatedAt).ToList();

            // Add data rows
            int row = 2;
            foreach (var order in orders)
            {
                worksheet.Cells[row, 1].Value = order.HotelOrderId;
                worksheet.Cells[row, 2].Value = order.User?.Email ?? "未知用户";
                worksheet.Cells[row, 3].Value = order.User?.Nickname ?? "未知用户";
                worksheet.Cells[row, 4].Value = order.RoomType?.Hotel?.Name ?? "未知酒店";
                worksheet.Cells[row, 5].Value = order.RoomType?.RoomTypeName ?? "未知房型";
                worksheet.Cells[row, 6].Value = order.CheckInDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 7].Value = order.CheckOutDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 8].Value = order.TotalPrice;
                worksheet.Cells[row, 9].Value = order.Status.ToString();
                worksheet.Cells[row, 10].Value = order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 11].Value = order.PaidAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
        }
    }

}
