using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace TourismPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("收到注册请求，邮箱: {Email}, 昵称: {Nickname}", model.Email, model.Nickname);

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("注册数据验证失败: {Errors}", errors);
                return View(model);
            }

            try
            {
                // 修复点1：直接调用，不再使用解构 (success, message)
                // Service 层会在失败时直接抛出 InvalidOperationException 异常
                var user = await _userService.RegisterAsync(model.Email, model.Password, model.Nickname);

                if (user != null)
                {
                    _logger.LogInformation("注册成功！用户ID: {UserId}", user.UserId);
                    TempData["Success"] = "注册成功，请登录！";
                    return RedirectToAction("Login");
                }
            }
            catch (InvalidOperationException ex) // 捕获业务逻辑错误（如：邮箱已存在、密码太弱）
            {
                _logger.LogWarning("注册被业务层拒绝: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message); // 将错误显示给用户
            }
            catch (Exception ex) // 捕获其他未预料的系统错误
            {
                _logger.LogError(ex, "注册发生系统异常");
                ModelState.AddModelError(string.Empty, "注册服务暂时不可用，请稍后重试。");
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("尝试登录: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.LoginAsync(model.Email, model.Password);
            if (user != null)
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "该账号已被禁用");
                    return View(model);
                }

                // 登录成功，写入 Session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserNickname", user.Nickname);
                HttpContext.Session.SetString("UserEmail", user.Email);
                
                // 修复点2：User 模型没有 Role 字段，且这是前台用户，直接标记为 "User"
                HttpContext.Session.SetString("UserRole", "User"); 

                _logger.LogInformation("登录成功: {Email}", model.Email);
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("登录失败 - 用户名或密码错误: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "邮箱或密码错误");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            _logger.LogInformation("用户注销: {Email}", email ?? "Unknown");

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}