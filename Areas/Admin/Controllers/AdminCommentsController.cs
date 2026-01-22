using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCommentsController : Controller
    {
        private readonly ICommentService _commentService;

        public AdminCommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminUserId").HasValue;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var comments = await _commentService.GetAllCommentsAsync();
            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            try
            {
                var success = await _commentService.DeleteAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "评论删除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "评论不存在";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除评论失败: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
