using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IDiaryService
    {
        Task<TravelDiary> CreateAsync(int userId, string title, string content, List<IFormFile> images);
        Task<bool> UpdateAsync(int diaryId, string title, string content);
        Task<bool> DeleteAsync(int diaryId);
        Task<TravelDiary> GetByIdAsync(int diaryId);
        Task<IEnumerable<TravelDiary>> GetAllAsync();
        Task<IEnumerable<TravelDiary>> GetUserDiariesAsync(int userId);
    }
}
