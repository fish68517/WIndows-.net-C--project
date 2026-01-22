using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using System.Security.Cryptography;
using System.Text;
using TourismPlatform.Models;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountController : Controller
    {
        private readonly MyDbContext _context;

        public AdminAccountController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to admin home
            if (HttpContext.Session.GetInt32("AdminUserId").HasValue)
            {
                return RedirectToAction("Index", "AdminHome");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find admin user by Email
            var adminUser = _context.AdminUsers
                .FirstOrDefault(a => a.Email == model.Email);

            if (adminUser == null || !VerifyPassword(model.Password, adminUser.PasswordHash))
            {
                ModelState.AddModelError("", "用户名或密码错误");
                return View(model);
            }

            if (!adminUser.IsActive)
            {
                ModelState.AddModelError("", "该管理员账户已被禁用");
                return View(model);
            }

            // Set admin session
            HttpContext.Session.SetInt32("AdminUserId", adminUser.AdminUserId);
        
            HttpContext.Session.SetString("AdminEmail", adminUser.Email);

            return RedirectToAction("Index", "AdminHome");
        }

        // 修改前：
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Logout() { ... }

        // 修改后：允许 GET 请求，这样 <a href="..."> 也能调用
        [HttpGet] 
        public IActionResult Logout()
        {
            // 清除管理员的 Session
            HttpContext.Session.Remove("AdminUserId");
            HttpContext.Session.Remove("AdminEmail");
            HttpContext.Session.Remove("UserRole"); // 如果有通用角色字段也清除

            // 或者暴力一点，清除所有：
            // HttpContext.Session.Clear(); 

            // 跳转回登录页  http://localhost:5000/Account/Login
            // 正确写法：明确指定 Action, Controller, 和 RouteValues (把 area 设为空)
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                // Hash the input password and compare with stored hash
                var hashOfInput = HashPassword(password);
                return hashOfInput == hash;
            }
            catch
            {
                return false;
            }
        }

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
