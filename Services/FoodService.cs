using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class FoodService : IFoodService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Food>> GetByAttractionAsync(int attractionId)
        {
            return await _unitOfWork.Foods.FindAsync(f => f.AttractionId == attractionId);
        }

        public async Task<IEnumerable<Food>> GetByDistrictAsync(int districtId)
        {
            return await _unitOfWork.Foods.FindAsync(f => f.DistrictId == districtId);
        }

        public async Task<IEnumerable<Food>> GetAllAsync()
        {
            return await _unitOfWork.Foods.GetAllAsync();
        }

        public async Task<Food> GetByIdAsync(int id)
        {
            return await _unitOfWork.Foods.GetByIdAsync(id);
        }

        public async Task<Food> CreateAsync(Food food)
        {
            food.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Foods.AddAsync(food);
            await _unitOfWork.SaveChangesAsync();
            return food;
        }

        public async Task<bool> UpdateAsync(Food food)
        {
            _unitOfWork.Foods.Update(food);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var food = await _unitOfWork.Foods.GetByIdAsync(id);
            if (food == null)
                return false;

            _unitOfWork.Foods.Remove(food);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
