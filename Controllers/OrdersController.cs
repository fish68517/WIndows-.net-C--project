using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using TourismPlatform.Repositories;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Threading.Tasks;
using TourismPlatform.Services.Alipay;
using Newtonsoft.Json; 
using System.Collections.Generic;

namespace TourismPlatform.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IAttractionService _attractionService;
        private readonly IHotelService _hotelService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrdersController> _logger;
        private readonly IAlipayService _alipayService;

        public OrdersController(
            IOrderService orderService, 
            IAttractionService attractionService, 
            IHotelService hotelService, 
            IUnitOfWork unitOfWork,
            ILogger<OrdersController> logger,
            IAlipayService alipayService)
        {
            _orderService = orderService;
            _attractionService = attractionService;
            _hotelService = hotelService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _alipayService = alipayService;
        }

        #region 门票业务 (Ticket Logic) - 保持原有逻辑

        // GET: /Orders/CreateTicket?attractionId=5
        [HttpGet]
        public async Task<IActionResult> CreateTicket(int attractionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var attraction = await _attractionService.GetByIdAsync(attractionId);
            if (attraction == null) return NotFound("景点不存在");

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

        // POST: /Orders/CreateTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CreateTicket")] 
        public async Task<IActionResult> CreateTicketConfirm(CreateTicketOrderViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                await ReloadTicketModel(model);
                return View(model);
            }

            try
            {
                var order = await _orderService.CreateTicketOrderAsync(
                    userId.Value, model.AttractionId, model.VisitDate, model.Quantity);

                if (order == null)
                {
                    ModelState.AddModelError("", "创建订单失败");
                    await ReloadTicketModel(model);
                    return View(model);
                }
                return RedirectToAction("PayTicket", new { orderId = order.TicketOrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建门票订单异常");
                ModelState.AddModelError("", "系统繁忙");
                await ReloadTicketModel(model);
                return View(model);
            }
        }

        // GET: /Orders/PayTicket
        [HttpGet]
        public async Task<IActionResult> PayTicket(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var order = await _orderService.GetTicketOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value) return NotFound();

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

        // POST: /Orders/PayTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("PayTicket")]
        public async Task<IActionResult> PayTicketConfirmed(int orderId, string paymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            if (string.IsNullOrEmpty(paymentMethod)) paymentMethod = "alipay"; 

            var order = await _orderService.GetTicketOrderAsync(orderId);
            if (order == null) return NotFound();

            // 支付宝支付 (门票直接传 ID)
            if (paymentMethod == "alipay")
            {
                var returnUrl = Url.Action("AlipayReturn", "Orders", null, Request.Scheme);
                var formHtml = _alipayService.GeneratePagePayRequest(
                    order.TicketOrderId.ToString(), 
                    order.TotalPrice.ToString("F2"),
                    $"门票-{order.Attraction?.Name}",
                    returnUrl
                );
                return Content(formHtml, "text/html");
            }

            // 模拟直接支付
            await _orderService.PayTicketOrderAsync(orderId);
            return RedirectToAction("Detail", new { orderId = orderId });
        }

        // GET: /Orders/Detail
        [HttpGet]
        public async Task<IActionResult> Detail(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var order = await _orderService.GetTicketOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value) return NotFound();

            var model = new TicketOrderDetailViewModel
            {
                TicketOrderId = order.TicketOrderId,
                AttractionName = order.Attraction?.Name ?? "未知",
                VisitDate = order.VisitDate,
                Quantity = order.Quantity,
                TotalPrice = order.TotalPrice,
                Status = ((int)order.Status).ToString(), 
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                VerifyCode = order.VerifyCode?.Code ?? "未生成"
            };
            return View(model);
        }

        #endregion

        #region 酒店业务 (Hotel Logic - 新增部分，修复 404 的关键)

        // GET: /Orders/CreateHotel?hotelId=1&roomTypeId=2
        [HttpGet]
        public async Task<IActionResult> CreateHotel(int hotelId, int roomTypeId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            // 获取房型信息
            var roomType = await _unitOfWork.HotelRoomTypes.GetByIdAsync(roomTypeId);
            if (roomType == null) return NotFound("房型不存在");
            
            // 获取酒店信息（为了获取酒店名称等）
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            if (hotel == null) return NotFound("酒店不存在");

            var model = new CreateHotelOrderViewModel
            {
                HotelId = hotelId,
                RoomTypeId = roomTypeId,
                HotelName = hotel.Name,
                RoomTypeName = roomType.RoomTypeName,
                PricePerNight = roomType.PricePerNight,
                // 默认入住今晚，明晚离店
                CheckInDate = DateTime.Now.Date,
                CheckOutDate = DateTime.Now.AddDays(1).Date
            };
            return View(model);
        }

        // POST: /Orders/CreateHotel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHotel(CreateHotelOrderViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            // 基础校验
            if (model.CheckOutDate <= model.CheckInDate)
            {
                ModelState.AddModelError("", "离店日期必须晚于入住日期");
            }

            if (!ModelState.IsValid) 
            {
                return View(model);
            }

            try
            {
                var order = await _orderService.CreateHotelOrderAsync(
                    userId.Value, model.RoomTypeId, model.CheckInDate, model.CheckOutDate);

                if (order == null)
                {
                    ModelState.AddModelError("", "创建订单失败，可能房型库存不足");
                    return View(model);
                }
                // 下单成功，跳转到酒店支付页
                return RedirectToAction("PayHotel", new { orderId = order.HotelOrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建酒店订单异常");
                ModelState.AddModelError("", "系统繁忙，请稍后重试");
                return View(model);
            }
        }

        // GET: /Orders/PayHotel?orderId=5
        [HttpGet]
        public async Task<IActionResult> PayHotel(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var order = await _orderService.GetHotelOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value) return NotFound();

            // 尝试获取酒店名称用于显示 (如果 Service 没 Include，这里补救)
            var hotelName = "未知酒店";
            if (order.RoomType != null)
            {
                // 如果 RoomType.Hotel 为空，查询一下
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(order.RoomType.HotelId);
                hotelName = hotel?.Name;
            }

            var model = new PayHotelOrderViewModel
            {
                HotelOrderId = order.HotelOrderId,
                HotelName = hotelName,
                RoomTypeName = order.RoomType?.RoomTypeName ?? "未知房型",
                CheckInDate = order.CheckInDate,
                CheckOutDate = order.CheckOutDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString()
            };
            return View(model);
        }

        // POST: /Orders/PayHotel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayHotel(int orderId, string paymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");
            if (string.IsNullOrEmpty(paymentMethod)) paymentMethod = "alipay";

            var order = await _orderService.GetHotelOrderAsync(orderId);
            if (order == null) return NotFound();

            // 支付宝支付 (酒店)
            if (paymentMethod == "alipay")
            {
                var returnUrl = Url.Action("AlipayReturn", "Orders", null, Request.Scheme);
                
                // 【关键】酒店订单号前加 "H" 前缀，如 "H1001"
                // 这样在回调时就能区分是 Ticket(纯数字) 还是 Hotel(H开头)
                var outTradeNo = "H" + order.HotelOrderId;
                
                var hotelName = "酒店预订";
                if (order.RoomType != null)
                {
                     var hotel = await _unitOfWork.Hotels.GetByIdAsync(order.RoomType.HotelId);
                     hotelName = $"{hotel?.Name}-{order.RoomType.RoomTypeName}";
                }

                var formHtml = _alipayService.GeneratePagePayRequest(
                    outTradeNo, 
                    order.TotalPrice.ToString("F2"),
                    hotelName,
                    returnUrl
                );
                return Content(formHtml, "text/html");
            }

            // 模拟直接支付
            await _orderService.PayHotelOrderAsync(orderId);
            return RedirectToAction("DetailHotel", new { orderId = orderId });
        }

        // GET: /Orders/DetailHotel
        [HttpGet]
        public async Task<IActionResult> DetailHotel(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var order = await _orderService.GetHotelOrderAsync(orderId);
            if (order == null || order.UserId != userId.Value) return NotFound();

            var hotelName = "未知酒店";
            if (order.RoomType != null)
            {
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(order.RoomType.HotelId);
                hotelName = hotel?.Name;
            }

            var model = new HotelOrderDetailViewModel
            {
                HotelOrderId = order.HotelOrderId,
                HotelName = hotelName,
                RoomTypeName = order.RoomType?.RoomTypeName,
                CheckInDate = order.CheckInDate,
                CheckOutDate = order.CheckOutDate,
                TotalPrice = order.TotalPrice,
                // 转为 int 字符串，例如 "1" 代表 Paid
                Status = ((int)order.Status).ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                VerifyCode = order.VerifyCode?.Code ?? "未生成"
            };
            return View(model);
        }

        #endregion

        // GET: /Orders/List (整合门票和酒店)
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var ticketOrders = await _orderService.GetUserTicketOrdersAsync(userId.Value);
            var hotelOrders = await _orderService.GetUserHotelOrdersAsync(userId.Value);

            var model = new UserOrderListViewModel
            {
                TicketOrders = ticketOrders.Select(o => new TicketOrderListItemViewModel
                {
                    TicketOrderId = o.TicketOrderId,
                    AttractionName = o.Attraction?.Name ?? "未知景点",
                    VisitDate = o.VisitDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                }).OrderByDescending(x => x.CreatedAt).ToList(),

                HotelOrders = hotelOrders.Select(o => new HotelOrderListItemViewModel
                {
                    HotelOrderId = o.HotelOrderId,
                    HotelName = o.RoomType?.Hotel?.Name ?? "未知酒店",
                    RoomTypeName = o.RoomType?.RoomTypeName ?? "未知房型",
                    CheckInDate = o.CheckInDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                }).OrderByDescending(x => x.CreatedAt).ToList()
            };

            return View(model);
        }

        // =============================================
        // 通用：支付宝回调 (Ticket + Hotel)
        // =============================================
        [HttpGet]
        public async Task<IActionResult> AlipayReturn()
        {
            var paramsMap = new Dictionary<string, string>();
            foreach (var key in Request.Query.Keys)
            {
                paramsMap.Add(key, Request.Query[key]);
            }

            // 1. 验签
            var isValid = _alipayService.ValidateCallback(paramsMap);
            
            if (isValid)
            {
                // 2. 获取外部订单号
                var outTradeNo = Request.Query["out_trade_no"].ToString();
                
                // 3. 判断是否是酒店订单 (前缀 "H")
                if (outTradeNo.StartsWith("H"))
                {
                    // 去掉前缀，获取真实 ID
                    var idStr = outTradeNo.Substring(1); 
                    if (int.TryParse(idStr, out int hotelOrderId))
                    {
                        await _orderService.PayHotelOrderAsync(hotelOrderId);
                        TempData["Success"] = "酒店预订成功！";
                        return RedirectToAction("DetailHotel", new { orderId = hotelOrderId });
                    }
                }
                // 4. 否则是门票订单 (纯数字)
                else
                {
                    if (int.TryParse(outTradeNo, out int ticketOrderId))
                    {
                        await _orderService.PayTicketOrderAsync(ticketOrderId);
                        TempData["Success"] = "门票预订成功！";
                        return RedirectToAction("Detail", new { orderId = ticketOrderId });
                    }
                }
            }

            return Content("支付验证失败，请联系客服。");
        }

        // 辅助方法
        private async Task ReloadTicketModel(CreateTicketOrderViewModel model)
        {
            var attraction = await _attractionService.GetByIdAsync(model.AttractionId);
            if (attraction != null)
            {
                model.AttractionName = attraction.Name;
                model.TicketPrice = attraction.TicketPrice;
            }
        }
    }
}