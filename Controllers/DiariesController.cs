using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using System.ComponentModel.DataAnnotations;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class DiariesController : Controller
    {
        private readonly IDiaryService _diaryService;

        public DiariesController(IDiaryService diaryService)
        {
            _diaryService = diaryService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDiaryViewModel model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Convert IFormFileCollection to List<IFormFile>
                var imageList = model.Images?.ToList() ?? new List<IFormFile>();

                // Create diary
                var diary = await _diaryService.CreateAsync(
                    userId.Value,
                    model.Title,
                    model.Content,
                    imageList
                );

                // Redirect to diary detail page
                return RedirectToAction("Detail", new { id = diary.DiaryId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "发布游记时出错，请稍后重试");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var diary = await _diaryService.GetByIdAsync(id);
            if (diary == null)
            {
                return NotFound("游记不存在");
            }

            return View(diary);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var diaries = await _diaryService.GetAllAsync();
            return View(diaries);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var diary = await _diaryService.GetByIdAsync(id);
            if (diary == null)
            {
                return NotFound("游记不存在");
            }

            // Check if user is the author
            if (diary.UserId != userId.Value)
            {
                return Forbid("您没有权限编辑此游记");
            }

            var model = new EditDiaryViewModel
            {
                DiaryId = diary.DiaryId,
                Title = diary.Title,
                Content = diary.Content
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditDiaryViewModel model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.DiaryId)
            {
                return BadRequest("游记ID不匹配");
            }

            var diary = await _diaryService.GetByIdAsync(id);
            if (diary == null)
            {
                return NotFound("游记不存在");
            }

            // Check if user is the author
            if (diary.UserId != userId.Value)
            {
                return Forbid("您没有权限编辑此游记");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _diaryService.UpdateAsync(id, model.Title, model.Content);
                if (!success)
                {
                    ModelState.AddModelError("", "更新游记失败，请稍后重试");
                    return View(model);
                }

                return RedirectToAction("Detail", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "编辑游记时出错，请稍后重试");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            var adminUserId = HttpContext.Session.GetInt32("AdminUserId");

            if (!userId.HasValue && !adminUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var diary = await _diaryService.GetByIdAsync(id);
            if (diary == null)
            {
                return NotFound("游记不存在");
            }

            // Check if user is the author or admin
            bool isAuthor = userId.HasValue && diary.UserId == userId.Value;
            bool isAdmin = adminUserId.HasValue;

            if (!isAuthor && !isAdmin)
            {
                return Forbid("您没有权限删除此游记");
            }

            try
            {
                var success = await _diaryService.DeleteAsync(id);
                if (!success)
                {
                    TempData["Error"] = "删除游记失败，请稍后重试";
                    return RedirectToAction("Detail", new { id = id });
                }

                TempData["Success"] = "游记已删除";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "删除游记时出错，请稍后重试";
                return RedirectToAction("Detail", new { id = id });
            }
        }
    }

}
