using TourismPlatform.Models;
using TourismPlatform.Repositories;

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

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            var comments = await _unitOfWork.Comments.GetAllAsync();
            
            // Load User for each comment
            foreach (var comment in comments)
            {
                await _unitOfWork.Context.Entry(comment).Reference(c => c.User).LoadAsync();
            }
            
            return comments;
        }
    }
}
