using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using TourismPlatform.Repositories;
using System.ComponentModel.DataAnnotations;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IAttractionService _attractionService;
        private readonly IHotelService _hotelService;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IOrderService orderService, IAttractionService attractionService, IHotelService hotelService, IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _attractionService = attractionService;
            _hotelService = hotelService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> CreateTicket(int attractionId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get attraction details
            var attraction = await _attractionService.GetByIdAsync(attractionId);
            if (attraction == null)
            {
                return NotFound("景点不存在");
            }

            var model = new CreateTicketOrderViewModel
            {
                AttractionId = attractionId,
                AttractionName = attraction.Name,
                TicketPrice = attraction.TicketPrice,
                VisitDate = DateTime.Now.AddDays(1).Date,
                Quantity = 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CreateTicket")] // <--- 关键点1：告诉路由系统，虽然方法改名了，但 URL 还是用 CreateTicket
        public async Task<IActionResult> CreateTicketConfirm(CreateTicketOrderViewModel model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Reload attraction info
                var attraction = await _attractionService.GetByIdAsync(model.AttractionId);
                if (attraction != null)
                {
                    model.AttractionName = attraction.Name;
                    model.TicketPrice = attraction.TicketPrice;
                }
                return View(model);
            }

            // Validate order parameters
            if (model.Quantity <= 0 || model.Quantity > 100)
            {
                ModelState.AddModelError("Quantity", "购票数量必须在1-100之间");
                var attraction = await _attractionService.GetByIdAsync(model.AttractionId);
                if (attraction != null)
                {
                    model.AttractionName = attraction.Name;
                    model.TicketPrice = attraction.TicketPrice;
                }
                return View(model);
            }

            if (model.VisitDate < DateTime.Now.Date)
            {
                ModelState.AddModelError("VisitDate", "访问日期不能早于今天");
                var attraction = await _attractionService.GetByIdAsync(model.AttractionId);
                if (attraction != null)
                {
                    model.AttractionName = attraction.Name;
                    model.TicketPrice = attraction.TicketPrice;
                }
                return View(model);
            }

            try
            {
                // Create ticket order
                var order = await _orderService.CreateTicketOrderAsync(
                    userId.Value,
                    model.AttractionId,
                    model.VisitDate,
                    model.Quantity
                );

                if (order == null)
                {
                    ModelState.AddModelError("", "创建订单失败，景点不存在");
                    var attraction = await _attractionService.GetByIdAsync(model.AttractionId);
                    if (attraction != null)
                    {
                        model.AttractionName = attraction.Name;
                        model.TicketPrice = attraction.TicketPrice;
                    }
                    return View(model);
                }

                // Redirect to payment page
                return RedirectToAction("PayTicket", new { orderId = order.TicketOrderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "创建订单时出错，请稍后重试");
                var attraction = await _attractionService.GetByIdAsync(model.AttractionId);
                if (attraction != null)
                {
                    model.AttractionName = attraction.Name;
                    model.TicketPrice = attraction.TicketPrice;
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PayTicket(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetTicketOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound("订单不存在");
            }

            var model = new PayTicketOrderViewModel
            {
                TicketOrderId = order.TicketOrderId,
                AttractionName = order.Attraction?.Name ?? "未知景点",
                VisitDate = order.VisitDate,
                Quantity = order.Quantity,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("PayTicket")] // <--- 关键点1：告诉路由系统，虽然方法改名了，但 URL 还是用 PayTicket
        public async Task<IActionResult> PayTicketConfirmed(int orderId) // <--- 关键点2：修改方法名，避免冲突
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetTicketOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound("订单不存在");
            }

            try
            {
                // Process payment
                var paymentSuccess = await _orderService.PayTicketOrderAsync(orderId);
                
                if (paymentSuccess)
                {
                    // Redirect to order detail page
                    return RedirectToAction("Detail", new { orderId = orderId });
                }
                else
                {
                    // Payment failed
                    var model = new PayTicketOrderViewModel // 确保使用全名
                    {
                        TicketOrderId = order.TicketOrderId,
                        AttractionName = order.Attraction?.Name ?? "未知景点",
                        VisitDate = order.VisitDate,
                        Quantity = order.Quantity,
                        TotalPrice = order.TotalPrice,
                        Status = order.Status.ToString()
                    };
                    ModelState.AddModelError("", "支付失败，请检查订单状态后重试");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                var model = new TourismPlatform.Models.PayTicketOrderViewModel // 确保使用全名
                {
                    TicketOrderId = order.TicketOrderId,
                    AttractionName = order.Attraction?.Name ?? "未知景点",
                    VisitDate = order.VisitDate,
                    Quantity = order.Quantity,
                    TotalPrice = order.TotalPrice,
                    Status = order.Status.ToString()
                };
                ModelState.AddModelError("", "支付过程中出错，请稍后重试");
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetTicketOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound("订单不存在");
            }

            var model = new TicketOrderDetailViewModel
            {
                TicketOrderId = order.TicketOrderId,
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
        public async Task<IActionResult> CreateHotel(int roomTypeId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get room type details
            var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(roomTypeId);
            if (roomType == null)
            {
                return NotFound("房型不存在");
            }

            var hotel = roomType.Hotel;
            if (hotel == null)
            {
                return NotFound("酒店不存在");
            }

            var model = new CreateHotelOrderViewModel
            {
                RoomTypeId = roomTypeId,
                HotelName = hotel.Name,
                RoomTypeName = roomType.RoomTypeName,
                PricePerNight = roomType.PricePerNight,
                CheckInDate = DateTime.Now.AddDays(1).Date,
                CheckOutDate = DateTime.Now.AddDays(2).Date
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHotel(CreateHotelOrderViewModel model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Reload room type info
                var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(model.RoomTypeId);
                if (roomType != null)
                {
                    model.HotelName = roomType.Hotel?.Name ?? "未知酒店";
                    model.RoomTypeName = roomType.RoomTypeName;
                    model.PricePerNight = roomType.PricePerNight;
                }
                return View(model);
            }

            // Validate order parameters
            if (model.CheckInDate < DateTime.Now.Date)
            {
                ModelState.AddModelError("CheckInDate", "入住日期不能早于今天");
                var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(model.RoomTypeId);
                if (roomType != null)
                {
                    model.HotelName = roomType.Hotel?.Name ?? "未知酒店";
                    model.RoomTypeName = roomType.RoomTypeName;
                    model.PricePerNight = roomType.PricePerNight;
                }
                return View(model);
            }

            if (model.CheckOutDate <= model.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "离店日期必须晚于入住日期");
                var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(model.RoomTypeId);
                if (roomType != null)
                {
                    model.HotelName = roomType.Hotel?.Name ?? "未知酒店";
                    model.RoomTypeName = roomType.RoomTypeName;
                    model.PricePerNight = roomType.PricePerNight;
                }
                return View(model);
            }

            var nights = (model.CheckOutDate - model.CheckInDate).Days;
            if (nights > 30)
            {
                ModelState.AddModelError("CheckOutDate", "单次预订最多30晚");
                var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(model.RoomTypeId);
                if (roomType != null)
                {
                    model.HotelName = roomType.Hotel?.Name ?? "未知酒店";
                    model.RoomTypeName = roomType.RoomTypeName;
                    model.PricePerNight = roomType.PricePerNight;
                }
                return View(model);
            }

            try
            {
                // Create hotel order
                var order = await _orderService.CreateHotelOrderAsync(
                    userId.Value,
                    model.RoomTypeId,
                    model.CheckInDate,
                    model.CheckOutDate
                );

                if (order == null)
                {
                    ModelState.AddModelError("", "创建订单失败，房型不存在");
                    var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(model.RoomTypeId);
                    if (roomType != null)
                    {
                        model.HotelName = roomType.Hotel?.Name ?? "未知酒店";
                        model.RoomTypeName = roomType.RoomTypeName;
                        model.PricePerNight = roomType.PricePerNight;
                    }
                    return View(model);
                }

                // Redirect to payment page
                return RedirectToAction("PayHotel", new { orderId = order.HotelOrderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "创建订单时出错，请稍后重试");
                var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(model.RoomTypeId);
                if (roomType != null)
                {
                    model.HotelName = roomType.Hotel?.Name ?? "未知酒店";
                    model.RoomTypeName = roomType.RoomTypeName;
                    model.PricePerNight = roomType.PricePerNight;
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PayHotel(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetHotelOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound("订单不存在");
            }

            var model = new PayHotelOrderViewModel
            {
                HotelOrderId = order.HotelOrderId,
                HotelName = order.RoomType?.Hotel?.Name ?? "未知酒店",
                RoomTypeName = order.RoomType?.RoomTypeName ?? "未知房型",
                CheckInDate = order.CheckInDate,
                CheckOutDate = order.CheckOutDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("PayHotel")] // <--- 关键点1
        public async Task<IActionResult> PayHotelConfirmed(int orderId) // <--- 关键点2：改名
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetHotelOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound("订单不存在");
            }

            try
            {
                // Process payment
                var paymentSuccess = await _orderService.PayHotelOrderAsync(orderId);
                
                if (paymentSuccess)
                {
                    // Redirect to order detail page
                    return RedirectToAction("DetailHotel", new { orderId = orderId });
                }
                else
                {
                    // Payment failed
                    var model = new TourismPlatform.Models.PayHotelOrderViewModel // 确保使用全名
                    {
                        HotelOrderId = order.HotelOrderId,
                        HotelName = order.RoomType?.Hotel?.Name ?? "未知酒店",
                        RoomTypeName = order.RoomType?.RoomTypeName ?? "未知房型",
                        CheckInDate = order.CheckInDate,
                        CheckOutDate = order.CheckOutDate,
                        TotalPrice = order.TotalPrice,
                        Status = order.Status.ToString()
                    };
                    ModelState.AddModelError("", "支付失败，请检查订单状态后重试");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                var model = new TourismPlatform.Models.PayHotelOrderViewModel // 确保使用全名
                {
                    HotelOrderId = order.HotelOrderId,
                    HotelName = order.RoomType?.Hotel?.Name ?? "未知酒店",
                    RoomTypeName = order.RoomType?.RoomTypeName ?? "未知房型",
                    CheckInDate = order.CheckInDate,
                    CheckOutDate = order.CheckOutDate,
                    TotalPrice = order.TotalPrice,
                    Status = order.Status.ToString()
                };
                ModelState.AddModelError("", "支付过程中出错，请稍后重试");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DetailHotel(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetHotelOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound("订单不存在");
            }

            var model = new HotelOrderDetailViewModel
            {
                HotelOrderId = order.HotelOrderId,
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

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticketOrders = await _orderService.GetUserTicketOrdersAsync(userId.Value);
            var hotelOrders = await _orderService.GetUserHotelOrdersAsync(userId.Value);

            var model = new UserOrderListViewModel
            {
                TicketOrders = ticketOrders.Select(o => new TicketOrderListItemViewModel
                {
                    TicketOrderId = o.TicketOrderId,
                    AttractionName = o.Attraction?.Name ?? "未知景点",
                    VisitDate = o.VisitDate,
                    Quantity = o.Quantity,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                }).ToList(),
                HotelOrders = hotelOrders.Select(o => new HotelOrderListItemViewModel
                {
                    HotelOrderId = o.HotelOrderId,
                    HotelName = o.RoomType?.Hotel?.Name ?? "未知酒店",
                    RoomTypeName = o.RoomType?.RoomTypeName ?? "未知房型",
                    CheckInDate = o.CheckInDate,
                    CheckOutDate = o.CheckOutDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                }).ToList()
            };

            return View(model);
        }
    }

}

