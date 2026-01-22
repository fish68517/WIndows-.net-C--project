using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAnnouncementsController : Controller
    {
        private readonly MyDbContext _context;

        public AdminAnnouncementsController(MyDbContext context)
        {
            _context = context;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminUserId").HasValue;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var announcements = _context.Announcements.OrderByDescending(a => a.PublishedAt).ToList();
            return View(announcements);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (!ModelState.IsValid)
            {
                return View(announcement);
            }

            try
            {
                announcement.CreatedAt = DateTime.UtcNow;
                announcement.UpdatedAt = DateTime.UtcNow;
                announcement.PublishedAt = DateTime.UtcNow;
                
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "公告创建成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"创建公告失败: {ex.Message}");
                return View(announcement);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var announcement = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement announcement)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (id != announcement.AnnouncementId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(announcement);
            }

            try
            {
                var existingAnnouncement = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
                if (existingAnnouncement == null)
                {
                    return NotFound();
                }

                existingAnnouncement.Title = announcement.Title;
                existingAnnouncement.Content = announcement.Content;
                existingAnnouncement.PublishedAt = announcement.PublishedAt;
                existingAnnouncement.UpdatedAt = DateTime.UtcNow;

                _context.Announcements.Update(existingAnnouncement);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "公告更新成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新公告失败: {ex.Message}");
                return View(announcement);
            }
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
                var announcement = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
                if (announcement == null)
                {
                    return NotFound();
                }

                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "公告删除成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除公告失败: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
