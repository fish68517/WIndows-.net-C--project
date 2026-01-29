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

        // 🟢 修复 GetByAttractionAsync
        // 原因：FindAsync 不支持 include，所以改用 GetAllAsync 加载所有带关联的数据，再用 Where 过滤
        public async Task<IEnumerable<Food>> GetByAttractionAsync(int attractionId)
        {
            var allFoods = await _unitOfWork.Foods.GetAllAsync(
                f => f.District, 
                f => f.Attraction
            );
            return allFoods.Where(f => f.AttractionId == attractionId);
        }

        // 🟢 修复 GetByDistrictAsync
        public async Task<IEnumerable<Food>> GetByDistrictAsync(int districtId)
        {
            var allFoods = await _unitOfWork.Foods.GetAllAsync(
                f => f.District, 
                f => f.Attraction
            );
            return allFoods.Where(f => f.DistrictId == districtId);
        }

        // 🟢 GetAllAsync 保持不变（之前验证过是好的）
        public async Task<IEnumerable<Food>> GetAllAsync()
        {
            return await _unitOfWork.Foods.GetAllAsync(
                f => f.District, 
                f => f.Attraction
            );
        }

        // 🟢 修复 GetByIdAsync
        // 同样改用 GetAllAsync + FirstOrDefault 来确保加载关联数据
        public async Task<Food> GetByIdAsync(int id)
        {
            var allFoods = await _unitOfWork.Foods.GetAllAsync(
                f => f.District,
                f => f.Attraction
            );
            return allFoods.FirstOrDefault(f => f.FoodId == id);
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