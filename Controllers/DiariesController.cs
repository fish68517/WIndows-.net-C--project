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
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"📝 [调试] 开始处理发布游记请求: {DateTime.Now}");

            // 1. 检查登录
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                Console.WriteLine("❌ [调试] 用户未登录，跳转到登录页");
                return RedirectToAction("Login", "Account");
            }

            // 2. 检查模型校验状态
            // if (!ModelState.IsValid)
            // {
            //     Console.WriteLine("⚠️ [调试] ModelState 校验失败！具体错误如下：");
            //     foreach (var state in ModelState)
            //     {
            //         foreach (var error in state.Value.Errors)
            //         {
            //             Console.WriteLine($"   - 字段: {state.Key}, 错误: {error.ErrorMessage}, 异常: {error.Exception?.Message}");
            //         }
            //     }
            //     // 如果校验失败，会直接返回 View，导致页面“不跳转”
            //     return View(model);
            // }

            try
            {
               // Console.WriteLine($"✅ [调试] 数据校验通过。标题: {model.Title}, 图片数量: {model.Images?.Count ?? 0}");

                var imageList = new List<IFormFile>();

                // 3. 调用 Service 创建游记
                Console.WriteLine("⏳ [调试] 正在调用 Service 创建数据...");
                var diary = await _diaryService.CreateAsync(
                    userId.Value,
                    model.Title,
                    model.Content,
                    imageList
                );

                Console.WriteLine($"🎉 [调试] 游记创建成功！ID: {diary.DiaryId}");
                Console.WriteLine("🚀 [调试] 正在执行跳转 RedirectToAction(\"Index\")...");

                // ✅ 4. 执行跳转
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [调试] 发生严重异常: {ex.Message}");
                Console.WriteLine($"❌ [调试] 堆栈信息: {ex.StackTrace}");
                
                // 如果发生异常，也会返回 View，导致页面“不跳转”
                ModelState.AddModelError("", $"发布失败: {ex.Message}");
                return View(model);
            }
            finally
            {
                Console.WriteLine("--------------------------------------------------");
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
