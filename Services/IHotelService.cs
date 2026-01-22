using TourismPlatform.Models;

namespace TourismPlatform.Services
{
    public interface IHotelService
    {
        Task<IEnumerable<Hotel>> GetByAttractionAsync(int attractionId);
        Task<IEnumerable<Hotel>> GetByDistrictAsync(int districtId);
        Task<IEnumerable<Hotel>> GetAllAsync();
        Task<Hotel> GetByIdAsync(int id);
        Task<Hotel> CreateAsync(Hotel hotel);
        Task<bool> UpdateAsync(Hotel hotel);
        Task<bool> DeleteAsync(int id);
    }
}
