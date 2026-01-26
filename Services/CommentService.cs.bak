using TourismPlatform.Models;
using TourismPlatform.Repositories;
using TourismPlatform.Data; // 确保引用了你的 DbContext 命名空间

namespace TourismPlatform.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Comment> CreateAsync(int userId, int? attractionId, int? diaryId, string content, int? rating)
        {
            var comment = new Comment
            {
                UserId = userId,
                AttractionId = attractionId,
                DiaryId = diaryId,
                Content = content,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteAsync(int commentId)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment == null)
                return false;

            _unitOfWork.Comments.Remove(comment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<Comment> GetCommentByIdAsync(int commentId)
        {
            return await _unitOfWork.Comments.GetByIdAsync(commentId);
        }

        public async Task<IEnumerable<Comment>> GetAttractionCommentsAsync(int attractionId)
        {
            var comments = await _unitOfWork.Comments.FindAsync(c => c.AttractionId == attractionId);
            
            // Load User for each comment
            foreach (var comment in comments)
            {
                await _unitOfWork.Context.Entry(comment).Reference(c => c.User).LoadAsync();
            }
            
            return comments;
        }

        public async Task<IEnumerable<Comment>> GetDiaryCommentsAsync(int diaryId)
        {
            var comments = await _unitOfWork.Comments.FindAsync(c => c.DiaryId == diaryId);
            
            // Load User for each comment
            foreach (var comment in comments)
            {
                await _unitOfWork.Context.Entry(comment).Reference(c => c.User).LoadAsync();
            }
            
            return comments;
        }

        public async Task<IEnumerable<Comment>> GetUserCommentsAsync(int userId)
        {
            return await _unitOfWork.Comments.FindAsync(c => c.UserId == userId);
        }

        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            // 按时间倒序，关联查询用户、景点和游记信息
            return await _unitOfWork.Comments
                .Include(c => c.User)
                .Include(c => c.Attraction)
                .Include(c => c.Diary)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveCommentAsync(int commentId)
        {
            var comment = await _unitOfWork.Comments.FindAsync(commentId);
            if (comment == null) return false;

            comment.Status = 1; // 设为通过
            _unitOfWork.Comments.Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectCommentAsync(int commentId)
        {
            var comment = await _unitOfWork.Comments.FindAsync(commentId);
            if (comment == null) return false;

            comment.Status = 2; // 设为拒绝
            _unitOfWork.Comments.Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _unitOfWork.Comments.FindAsync(commentId);
            if (comment == null) return false;

            _unitOfWork.Comments.Remove(comment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
