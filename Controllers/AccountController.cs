using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using TourismPlatform.Data; // 1. 引入 Data 命名空间
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Cryptography; // 2. 引入加密命名空间
using System.Text;

namespace TourismPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;
        private readonly MyDbContext _context; // 3. 定义数据库上下文

        // 4. 在构造函数中注入 MyDbContext
        public AccountController(IUserService userService, ILogger<AccountController> logger, MyDbContext context)
        {
            _userService = userService;
            _logger = logger;
            _context = context;
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
                var user = await _userService.RegisterAsync(model.Email, model.Password, model.Nickname);

                if (user != null)
                {
                    _logger.LogInformation("注册成功！用户ID: {UserId}", user.UserId);
                    TempData["Success"] = "注册成功，请登录！";
                    return RedirectToAction("Login");
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("注册被业务层拒绝: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
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

            // ==========================================
            // 5. 管理员登录逻辑 (Admin)
            // ==========================================
            if (model.Email == "admin@tourism.com")
            {
                // 1. 在 AdminUsers 表中查找
                var admin = _context.AdminUsers.FirstOrDefault(a => a.Email == model.Email);
                
                if (admin != null)
                {
                    // 2. 验证密码 (使用 SHA256 哈希匹配)
                    var inputHash = HashPassword(model.Password);
                    if (admin.PasswordHash == inputHash)
                    {
                        if (!admin.IsActive)
                        {
                            ModelState.AddModelError(string.Empty, "管理员账号已被禁用");
                            return View(model);
                        }

                        // 3. 写入管理员 Session
                        HttpContext.Session.SetInt32("AdminUserId", admin.AdminUserId); // 设置后台专用ID
                        HttpContext.Session.SetString("UserEmail", admin.Email);
                        HttpContext.Session.SetString("UserNickname", admin.Username);
                        HttpContext.Session.SetString("UserRole", "Admin"); // 关键：标记为管理员角色

                        _logger.LogInformation("管理员登录成功: {Email}", model.Email);

                        // 4. 跳转到后台区域 (Areas/Admin)
                        return RedirectToAction("Index", "AdminHome", new { area = "Admin" });
                    }
                }

                _logger.LogWarning("管理员登录失败 - 密码错误");
                ModelState.AddModelError(string.Empty, "管理员密码错误");
                return View(model);
            }

            // ==========================================
            // 6. 普通用户登录逻辑 (User)
            // ==========================================
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
                HttpContext.Session.SetString("UserRole", "User"); // 标记为普通用户

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
            // return RedirectToAction("Index", "Login");
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        // ==========================================
        // 7. 辅助方法：SHA256 密码哈希
        // ==========================================
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}