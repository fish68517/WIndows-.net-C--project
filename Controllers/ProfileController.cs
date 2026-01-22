using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
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

            var model = new EditProfileViewModel
            {
                Nickname = user.Nickname,
                Bio = user.Bio
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _userService.UpdateProfileAsync(userId.Value, model.Nickname, model.Bio, model.Avatar);
                if (success)
                {
                    // Update session with new nickname
                    HttpContext.Session.SetString("UserNickname", model.Nickname);
                    TempData["SuccessMessage"] = "个人资料更新成功";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "更新失败，请稍后重试");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "更新失败，请稍后重试");
                return View(model);
            }
        }
    }

   
}
