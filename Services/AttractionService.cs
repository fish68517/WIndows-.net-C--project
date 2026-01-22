using Microsoft.EntityFrameworkCore;
using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class AttractionService : IAttractionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttractionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Attraction>> GetAllAsync()
        {
            return await _unitOfWork.Attractions.GetAllAsync(
                a => a.District,
                a => a.Images
            );
        }

        public async Task<IEnumerable<Attraction>> GetByDistrictAsync(int districtId)
        {
            return await _unitOfWork.Attractions.FindAsync(a => a.DistrictId == districtId);
        }

        public async Task<IEnumerable<Attraction>> GetByCategoryAsync(int categoryId)
        {
            return await _unitOfWork.Attractions.FindAsync(a => a.CategoryId == categoryId);
        }

        public async Task<IEnumerable<Attraction>> SearchAsync(string keyword)
        {
            return await _unitOfWork.Attractions.FindAsync(a => a.Name.Contains(keyword));
        }

        public async Task<Attraction> GetByIdAsync(int id)
        {
            var attraction = await _unitOfWork.Attractions.GetByIdAsync(id,
                a => a.District,
                a => a.Category,
                a => a.Images,
                a => a.Comments
            );
            
            if (attraction != null)
            {
                // Load User for each comment
                if (attraction.Comments != null && attraction.Comments.Any())
                {
                    foreach (var comment in attraction.Comments)
                    {
                        await _unitOfWork.Context.Entry(comment).Reference(c => c.User).LoadAsync();
                    }
                }
                
                // Increment view count
                attraction.ViewCount++;
                _unitOfWork.Attractions.Update(attraction);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return attraction;
        }

        public async Task<Attraction> CreateAsync(Attraction attraction)
        {
            attraction.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Attractions.AddAsync(attraction);
            await _unitOfWork.SaveChangesAsync();
            return attraction;
        }

        public async Task<bool> UpdateAsync(Attraction attraction)
        {
            _unitOfWork.Attractions.Update(attraction);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attraction = await _unitOfWork.Attractions.GetByIdAsync(id);
            if (attraction == null)
                return false;

            _unitOfWork.Attractions.Remove(attraction);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<District>> GetAllDistrictsAsync()
        {
            return await _unitOfWork.Districts.GetAllAsync();
        }

        public async Task<IEnumerable<AttractionCategory>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.AttractionCategories.GetAllAsync();
        }
    }
}
