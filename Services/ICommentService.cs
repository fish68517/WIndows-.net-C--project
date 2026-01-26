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
        


        Task<List<Comment>> GetAllCommentsAsync(); // 获取所有评论(包括待审核)
        Task<bool> ApproveCommentAsync(int commentId); // 通过
        Task<bool> RejectCommentAsync(int commentId);  // 拒绝
        Task<bool> DeleteCommentAsync(int commentId);  // 删除
    }
}
