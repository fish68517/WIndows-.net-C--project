using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using System.ComponentModel.DataAnnotations;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check email uniqueness
            var existingUser = await _userService.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "该邮箱已被注册");
                return View(model);
            }

            // Validate password strength
            if (!IsPasswordStrong(model.Password))
            {
                ModelState.AddModelError("Password", "密码必须至少8个字符，包含大小写字母和数字");
                return View(model);
            }

            try
            {
                var user = await _userService.RegisterAsync(model.Email, model.Password, model.Nickname);
                
                // Set user session or cookie
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserNickname", user.Nickname);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "注册失败，请稍后重试");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.LoginAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "邮箱或密码错误");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "该账户已被禁用");
                return View(model);
            }

            // Set user session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserNickname", user.Nickname);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpperCase && hasLowerCase && hasDigit;
        }
    }
}
