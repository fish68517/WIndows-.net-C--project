using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface ICommentService
    {
        Task<Comment> CreateAsync(int userId, int? attractionId, int? diaryId, string content, int? rating);
        Task<bool> DeleteAsync(int commentId);
        Task<Comment> GetCommentByIdAsync(int commentId);
        Task<IEnumerable<Comment>> GetAttractionCommentsAsync(int attractionId);
        Task<IEnumerable<Comment>> GetDiaryCommentsAsync(int diaryId);
        Task<IEnumerable<Comment>> GetUserCommentsAsync(int userId);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
    }
}
