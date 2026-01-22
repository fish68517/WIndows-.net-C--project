using Microsoft.AspNetCore.Mvc;

namespace TourismPlatform.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(); // 对应 Views/Home/Index.cshtml
        }
    }
}