using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class MeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IDiaryService _diaryService;
        private readonly ICommentService _commentService;
        private readonly IFavoriteService _favoriteService;

        public MeController(
            IUserService userService,
            IOrderService orderService,
            IDiaryService diaryService,
            ICommentService commentService,
            IFavoriteService favoriteService)
        {
            _userService = userService;
            _orderService = orderService;
            _diaryService = diaryService;
            _commentService = commentService;
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            // Get user statistics
            var ticketOrders = await _orderService.GetUserTicketOrdersAsync(userId.Value);
            var hotelOrders = await _orderService.GetUserHotelOrdersAsync(userId.Value);
            var diaries = await _diaryService.GetUserDiariesAsync(userId.Value);
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);

            var model = new PersonalDashboardViewModel
            {
                User = user,
                TicketOrderCount = ticketOrders?.Count() ?? 0,
                HotelOrderCount = hotelOrders?.Count() ?? 0,
                DiaryCount = diaries?.Count() ?? 0,
                FavoriteCount = favorites?.Count() ?? 0,
                RecentTicketOrders = ticketOrders?.OrderByDescending(o => o.CreatedAt).Take(3).ToList(),
                RecentHotelOrders = hotelOrders?.OrderByDescending(o => o.CreatedAt).Take(3).ToList(),
                RecentDiaries = diaries?.OrderByDescending(d => d.CreatedAt).Take(3).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyDiaries()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var diaries = await _diaryService.GetUserDiariesAsync(userId.Value);
            return View(diaries?.OrderByDescending(d => d.CreatedAt).ToList() ?? new List<Models.TravelDiary>());
        }

        [HttpGet]
        public async Task<IActionResult> MyComments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comments = await _commentService.GetUserCommentsAsync(userId.Value);
            return View(comments?.OrderByDescending(c => c.CreatedAt).ToList() ?? new List<Models.Comment>());
        }

        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);
            return View(favorites?.OrderByDescending(f => f.AttractionId).ToList() ?? new List<Models.Attraction>());
        }

        [HttpGet]
        public async Task<IActionResult> Tickets(string status = "")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetUserTicketOrdersAsync(userId.Value);
            var orderList = orders?.OrderByDescending(o => o.CreatedAt).ToList() ?? new List<Models.TicketOrder>();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                orderList = orderList.Where(o => o.Status.ToString() == status).ToList();
            }

            var model = new TicketOrdersViewModel
            {
                Orders = orderList,
                SelectedStatus = status
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Hotels(string status = "")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetUserHotelOrdersAsync(userId.Value);
            var orderList = orders?.OrderByDescending(o => o.CreatedAt).ToList() ?? new List<Models.HotelOrder>();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                orderList = orderList.Where(o => o.Status.ToString() == status).ToList();
            }

            var model = new HotelOrdersViewModel
            {
                Orders = orderList,
                SelectedStatus = status
            };

            return View(model);
        }
    }


}
