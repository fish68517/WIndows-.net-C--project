using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class DiaryService : IDiaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public DiaryService(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task<TravelDiary> CreateAsync(int userId, string title, string content, List<IFormFile> images)
        {
            var diary = new TravelDiary
            {
                UserId = userId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TravelDiaries.AddAsync(diary);
            await _unitOfWork.SaveChangesAsync();

            // Handle image uploads
            if (images != null && images.Count > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "diaries");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{image.FileName}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Store image URL in diary content or as metadata
                        // For now, we'll append image URLs to content
                        diary.Content += $"\n![image](/uploads/diaries/{fileName})";
                    }
                }

                _unitOfWork.TravelDiaries.Update(diary);
                await _unitOfWork.SaveChangesAsync();
            }

            return diary;
        }

        public async Task<bool> UpdateAsync(int diaryId, string title, string content)
        {
            var diary = await _unitOfWork.TravelDiaries.GetByIdAsync(diaryId);
            if (diary == null)
                return false;

            diary.Title = title;
            diary.Content = content;
            diary.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.TravelDiaries.Update(diary);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int diaryId)
        {
            var diary = await _unitOfWork.TravelDiaries.GetByIdAsync(diaryId);
            if (diary == null)
                return false;

            _unitOfWork.TravelDiaries.Remove(diary);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<TravelDiary> GetByIdAsync(int diaryId)
        {
            return await _unitOfWork.TravelDiaries.GetByIdAsync(diaryId, d => d.User, d => d.Comments);
        }

        public async Task<IEnumerable<TravelDiary>> GetAllAsync()
        {
            return await _unitOfWork.TravelDiaries.GetAllAsync(d => d.User, d => d.Comments);
        }

        public async Task<IEnumerable<TravelDiary>> GetUserDiariesAsync(int userId)
        {
            return await _unitOfWork.TravelDiaries.FindAsync(d => d.UserId == userId);
        }
    }
}
