using TourismPlatform.Models;
using TourismPlatform.Repositories;

namespace TourismPlatform.Services
{
    public class HotelService : IHotelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HotelService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Hotel>> GetByAttractionAsync(int attractionId)
        {
            return await _unitOfWork.Hotels.FindAsync(h => h.AttractionId == attractionId);
        }

        public async Task<IEnumerable<Hotel>> GetByDistrictAsync(int districtId)
        {
            return await _unitOfWork.Hotels.FindAsync(h => h.DistrictId == districtId);
        }

        public async Task<IEnumerable<Hotel>> GetAllAsync()
        {
            return await _unitOfWork.Hotels.GetAllAsync();
        }

        public async Task<Hotel> GetByIdAsync(int id)
        {
            return await _unitOfWork.Hotels.GetByIdAsync(id);
        }

        public async Task<Hotel> CreateAsync(Hotel hotel)
        {
            hotel.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Hotels.AddAsync(hotel);
            await _unitOfWork.SaveChangesAsync();
            return hotel;
        }

        public async Task<bool> UpdateAsync(Hotel hotel)
        {
            _unitOfWork.Hotels.Update(hotel);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
            if (hotel == null)
                return false;

            _unitOfWork.Hotels.Remove(hotel);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
