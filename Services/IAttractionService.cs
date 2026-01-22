using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IAttractionService
    {
        Task<IEnumerable<Attraction>> GetAllAsync();
        Task<IEnumerable<Attraction>> GetByDistrictAsync(int districtId);
        Task<IEnumerable<Attraction>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Attraction>> SearchAsync(string keyword);
        Task<Attraction> GetByIdAsync(int id);
        Task<Attraction> CreateAsync(Attraction attraction);
        Task<bool> UpdateAsync(Attraction attraction);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<District>> GetAllDistrictsAsync();
        Task<IEnumerable<AttractionCategory>> GetAllCategoriesAsync();
    }
}
