using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Data;
using TourismPlatform.Models;
using TourismPlatform.Services;
using System.IO;

namespace TourismPlatform.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAttractionsController : Controller
    {
        private readonly IAttractionService _attractionService;
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminAttractionsController(IAttractionService attractionService, MyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _attractionService = attractionService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

            var attractions = await _attractionService.GetAllAsync();
            return View(attractions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var districts = await _attractionService.GetAllDistrictsAsync();
            var categories = await _attractionService.GetAllCategoriesAsync();

            ViewBag.Districts = districts;
            ViewBag.Categories = categories;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attraction attraction, List<IFormFile> images)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (!ModelState.IsValid)
            {
                var districts = await _attractionService.GetAllDistrictsAsync();
                var categories = await _attractionService.GetAllCategoriesAsync();
                ViewBag.Districts = districts;
                ViewBag.Categories = categories;
                return View(attraction);
            }

            try
            {
                // Create the attraction
                var createdAttraction = await _attractionService.CreateAsync(attraction);

                // Handle image uploads
                if (images != null && images.Count > 0)
                {
                    int displayOrder = 0;
                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            var imageUrl = await SaveImageAsync(image);
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                var attractionImage = new AttractionImage
                                {
                                    AttractionId = createdAttraction.AttractionId,
                                    ImageUrl = imageUrl,
                                    DisplayOrder = displayOrder++
                                };
                                _context.AttractionImages.Add(attractionImage);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "景点创建成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"创建景点失败: {ex.Message}");
                var districts = await _attractionService.GetAllDistrictsAsync();
                var categories = await _attractionService.GetAllCategoriesAsync();
                ViewBag.Districts = districts;
                ViewBag.Categories = categories;
                return View(attraction);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var attraction = await _attractionService.GetByIdAsync(id);
            if (attraction == null)
            {
                return NotFound();
            }

            var districts = await _attractionService.GetAllDistrictsAsync();
            var categories = await _attractionService.GetAllCategoriesAsync();

            ViewBag.Districts = districts;
            ViewBag.Categories = categories;

            return View(attraction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Attraction attraction, List<IFormFile> newImages)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            if (id != attraction.AttractionId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var districts = await _attractionService.GetAllDistrictsAsync();
                var categories = await _attractionService.GetAllCategoriesAsync();
                ViewBag.Districts = districts;
                ViewBag.Categories = categories;
                return View(attraction);
            }

            try
            {
                // Update the attraction
                await _attractionService.UpdateAsync(attraction);

                // Handle new image uploads
                if (newImages != null && newImages.Count > 0)
                {
                    var existingImages = _context.AttractionImages
                        .Where(ai => ai.AttractionId == id)
                        .ToList();
                    
                    int displayOrder = existingImages.Count;
                    foreach (var image in newImages)
                    {
                        if (image.Length > 0)
                        {
                            var imageUrl = await SaveImageAsync(image);
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                var attractionImage = new AttractionImage
                                {
                                    AttractionId = id,
                                    ImageUrl = imageUrl,
                                    DisplayOrder = displayOrder++
                                };
                                _context.AttractionImages.Add(attractionImage);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "景点更新成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新景点失败: {ex.Message}");
                var districts = await _attractionService.GetAllDistrictsAsync();
                var categories = await _attractionService.GetAllCategoriesAsync();
                ViewBag.Districts = districts;
                ViewBag.Categories = categories;
                return View(attraction);
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
                // Delete associated images from file system
                var images = _context.AttractionImages
                    .Where(ai => ai.AttractionId == id)
                    .ToList();

                foreach (var image in images)
                {
                    DeleteImageFile(image.ImageUrl);
                }

                // Delete the attraction
                await _attractionService.DeleteAsync(id);

                TempData["SuccessMessage"] = "景点删除成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"删除景点失败: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "未授权" });
            }

            try
            {
                var image = _context.AttractionImages.FirstOrDefault(ai => ai.ImageId == imageId);
                if (image == null)
                {
                    return Json(new { success = false, message = "图片不存在" });
                }

                DeleteImageFile(image.ImageUrl);
                _context.AttractionImages.Remove(image);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "图片删除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"删除失败: {ex.Message}" });
            }
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "attractions");
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return relative path for storage in database
                return $"/uploads/attractions/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"图片上传失败: {ex.Message}");
            }
        }

        private void DeleteImageFile(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                // Remove leading slash if present
                var relativePath = imageUrl.StartsWith("/") ? imageUrl.Substring(1) : imageUrl;
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - file deletion failure shouldn't block the operation
                Console.WriteLine($"删除图片文件失败: {ex.Message}");
            }
        }
    }
}
