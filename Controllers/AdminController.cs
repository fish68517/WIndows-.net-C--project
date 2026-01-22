using Microsoft.AspNetCore.Mvc;

namespace TourismPlatform.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}