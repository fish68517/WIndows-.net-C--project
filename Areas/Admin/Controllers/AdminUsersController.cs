using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminUsersController : Controller
    {
        private readonly IUserService _userService;

        public AdminUsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminUserId").HasValue;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string nickname = null, DateTime? registeredAfter = null, DateTime? registeredBefore = null)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            IEnumerable<Models.User> users;

            // If any filter is applied, use search; otherwise get all users
            if (!string.IsNullOrEmpty(nickname) || registeredAfter.HasValue || registeredBefore.HasValue)
            {
                users = await _userService.SearchUsersAsync(nickname, registeredAfter, registeredBefore);
            }
            else
            {
                users = await _userService.GetAllUsersAsync();
                users = users.OrderByDescending(u => u.CreatedAt);
            }

            // Pass filter values to view for form persistence
            ViewBag.FilterNickname = nickname;
            ViewBag.FilterRegisteredAfter = registeredAfter?.ToString("yyyy-MM-dd");
            ViewBag.FilterRegisteredBefore = registeredBefore?.ToString("yyyy-MM-dd");

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            try
            {
                var result = await _userService.DisableUserAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "用户已禁用";
                }
                else
                {
                    TempData["ErrorMessage"] = "用户不存在";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"禁用用户失败: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            try
            {
                var result = await _userService.EnableUserAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "用户已启用";
                }
                else
                {
                    TempData["ErrorMessage"] = "用户不存在";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"启用用户失败: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
