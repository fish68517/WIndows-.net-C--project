using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly IFavoriteService _favoriteService;
  private readonly ILogger<FavoritesController> _logger; // 定义日志对象

        // 注入 ILogger
        public FavoritesController(IFavoriteService favoriteService, ILogger<FavoritesController> logger)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        [HttpPost]
        // [ValidateAntiForgeryToken] // 建议：调试阶段先注释掉这一行，防止因为 Token 问题直接被拦截，导致日志都不打印
        public async Task<IActionResult> Add(int attractionId)
        {
            _logger.LogInformation($"[Favorites/Add] 接收到请求，参数 attractionId: {attractionId}");

            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            _logger.LogInformation($"[Favorites/Add] 当前 Session UserId: {userId}");

            if (!userId.HasValue)
            {
                _logger.LogWarning("[Favorites/Add] 用户未登录");
                
                // 【关键修改】不要 Redirect，而是返回 JSON 告诉前端
                // 如果前端收到 success: false 且 message 包含登录提示，前端再处理跳转
                return Json(new { success = false, message = "请先登录", requireLogin = true });
            }

            try
            {
                var result = await _favoriteService.AddAsync(userId.Value, attractionId);
                _logger.LogInformation($"[Favorites/Add] Service返回结果: {result}");
                
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
                _logger.LogError(ex, "[Favorites/Add] 发生异常");
                return Json(new { success = false, message = "收藏失败，请稍后重试" });
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken] // 调试阶段先注释掉
        public async Task<IActionResult> Remove(int attractionId)
        {
            _logger.LogInformation($"[Favorites/Remove] 接收到请求，参数 attractionId: {attractionId}");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogWarning("[Favorites/Remove] 用户未登录");
                return Json(new { success = false, message = "请先登录", requireLogin = true });
            }

            try
            {
                var result = await _favoriteService.RemoveAsync(userId.Value, attractionId);
                _logger.LogInformation($"[Favorites/Remove] Service返回结果: {result}");

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
                _logger.LogError(ex, "[Favorites/Remove] 发生异常");
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
