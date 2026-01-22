using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminDiariesController : Controller
    {
        private readonly IDiaryService _diaryService;

        public AdminDiariesController(IDiaryService diaryService)
        {
            _diaryService = diaryService;
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

            var diaries = await _diaryService.GetAllAsync();
            return View(diaries);
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
                var success = await _diaryService.DeleteAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "游记删除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "游记不存在";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除游记失败: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
