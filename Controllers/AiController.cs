using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services.Ai;

namespace TourismPlatform.Controllers
{
    public class AiController : Controller
    {
        private readonly SiliconFlowService _aiService;

        public AiController(SiliconFlowService aiService)
        {
            _aiService = aiService;
        }

        // 对应前端页面
        public IActionResult Index()
        {
            return View();
        }

        // 对应前端 AJAX 请求: $.ajax({ url: '/Ai/SendMessage', ... })
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, answer = "请输入内容" });
            }

            try
            {
                // 调用服务获取回答
                var answer = await _aiService.GetChatResponseAsync(message);
                
                // 返回前端需要的 JSON 格式
                return Json(new { success = true, answer = answer });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, answer = $"系统错误: {ex.Message}" });
            }
        }
    }
}