using Microsoft.AspNetCore.Mvc;

namespace TourismPlatform.Controllers
{
    public class MeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Orders()
        {
            return View();
        }
    }
}