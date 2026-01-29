using Microsoft.AspNetCore.Mvc;
using TourismPlatform.Models;
using TourismPlatform.Services;
using System.ComponentModel.DataAnnotations;
using TourismPlatform.Models;

namespace TourismPlatform.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IOrderService _orderService;

        public CommentsController(ICommentService commentService, IOrderService orderService)
        {
            _commentService = commentService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int attractionId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user has a "Used" status ticket order for this attraction
            var userOrders = await _orderService.GetUserTicketOrdersAsync(userId.Value);
            var hasUsedOrder = userOrders.Any(o => o.AttractionId == attractionId && o.Status == TicketOrderStatus.Used);

            // if (!hasUsedOrder)
            // {
            //     return BadRequest("只有使用过该景点门票的用户才能评论");
            // }

            var model = new CreateCommentViewModel
            {
                AttractionId = attractionId,
                Rating = 5
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommentViewModel model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if user has a "Used" status ticket order for this attraction
            var userOrders = await _orderService.GetUserTicketOrdersAsync(userId.Value);
            var hasUsedOrder = userOrders.Any(o => o.AttractionId == model.AttractionId && o.Status == TicketOrderStatus.Used);

            // if (!hasUsedOrder)
            // {
            //     return BadRequest("只有使用过该景点门票的用户才能评论");
            // }

            // Validate rating
            if (model.Rating < 1 || model.Rating > 5)
            {
                ModelState.AddModelError("Rating", "评分必须在1-5星之间");
                return View(model);
            }

            try
            {
                // Create comment
                var comment = await _commentService.CreateAsync(
                    userId.Value,
                    model.AttractionId,
                    null,
                    model.Content,
                    model.Rating
                );

                // Redirect back to attraction detail page
                return RedirectToAction("Detail", "Attractions", new { id = model.AttractionId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "创建评论时出错，请稍后重试");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiaryComment(int diaryId, string content)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("评论内容不能为空");
            }

            try
            {
                // Create comment
                var comment = await _commentService.CreateAsync(
                    userId.Value,
                    null,
                    diaryId,
                    content,
                    null
                );

                // Redirect back to diary detail page
                return RedirectToAction("Detail", "Diaries", new { id = diaryId });
            }
            catch (Exception ex)
            {
                return BadRequest("创建评论时出错，请稍后重试");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int commentId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            var adminUserId = HttpContext.Session.GetInt32("AdminUserId");

            if (!userId.HasValue && !adminUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the comment
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound("评论不存在");
            }

            // Check authorization: only comment author or admin can delete
            bool isAuthor = userId.HasValue && comment.UserId == userId.Value;
            bool isAdmin = adminUserId.HasValue;

            if (!isAuthor && !isAdmin)
            {
                return Forbid("您没有权限删除此评论");
            }

            try
            {
                // Delete the comment
                await _commentService.DeleteAsync(commentId);

                // Redirect back to the attraction detail page if it's an attraction comment
                if (comment.AttractionId.HasValue)
                {
                    return RedirectToAction("Detail", "Attractions", new { id = comment.AttractionId });
                }
                // Redirect back to the diary detail page if it's a diary comment
                else if (comment.DiaryId.HasValue)
                {
                    return RedirectToAction("Detail", "Diaries", new { id = comment.DiaryId });
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest("删除评论时出错，请稍后重试");
            }
        }
    }
}
