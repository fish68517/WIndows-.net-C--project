using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Services;
using TourismPlatform.Models;
namespace TourismPlatform.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return Json(new { success = false, message = "问题不能为空" });
            }

            var answer = await _chatService.GetAnswerAsync(question);
            return Json(new { success = true, answer = answer });
        }
    }
}
