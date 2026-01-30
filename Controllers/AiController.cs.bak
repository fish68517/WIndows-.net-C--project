using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TourismPlatform.Services.Ai;

namespace TourismPlatform.Controllers
{
    public class AiController : Controller
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        // 页面视图
        public IActionResult Index()
        {
            return View();
        }

        // AJAX 接口：发送消息
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return Json(new { success = false, answer = "请输入内容" });

            var answer = await _aiService.GetAnswerAsync(message);
            return Json(new { success = true, answer = answer });
        }
    }
}