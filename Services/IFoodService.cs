using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IFoodService
    {
        Task<IEnumerable<Food>> GetByAttractionAsync(int attractionId);
        Task<IEnumerable<Food>> GetByDistrictAsync(int districtId);
        Task<IEnumerable<Food>> GetAllAsync();
        Task<Food> GetByIdAsync(int id);
        Task<Food> CreateAsync(Food food);
        Task<bool> UpdateAsync(Food food);
        Task<bool> DeleteAsync(int id);
    }
}
