using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int attractionId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _favoriteService.AddAsync(userId.Value, attractionId);
                
                if (result)
                {
                    return Json(new { success = true, message = "收藏成功" });
                }
                else
                {
                    return Json(new { success = false, message = "该景点已被收藏" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "收藏失败，请稍后重试" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int attractionId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _favoriteService.RemoveAsync(userId.Value, attractionId);
                
                if (result)
                {
                    return Json(new { success = true, message = "取消收藏成功" });
                }
                else
                {
                    return Json(new { success = false, message = "该景点未被收藏" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "取消收藏失败，请稍后重试" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);
                return View(favorites);
            }
            catch (Exception ex)
            {
                return BadRequest("加载收藏列表失败，请稍后重试");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsFavorited(int attractionId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { isFavorited = false });
            }

            try
            {
                var isFavorited = await _favoriteService.IsFavoritedAsync(userId.Value, attractionId);
                return Json(new { isFavorited = isFavorited });
            }
            catch (Exception ex)
            {
                return Json(new { isFavorited = false });
            }
        }
    }
}
