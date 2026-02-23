using TourismPlatform.Models;
using TourismPlatform.Repositories;


using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

namespace TourismPlatform.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FavoriteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddAsync(int userId, int attractionId)
        {
            var exists = await _unitOfWork.Favorites.AnyAsync(f => f.UserId == userId && f.AttractionId == attractionId);
            if (exists)
                return false;

            var favorite = new Favorite
            {
                UserId = userId,
                AttractionId = attractionId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Favorites.AddAsync(favorite);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAsync(int userId, int attractionId)
        {
            var favorite = await _unitOfWork.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.AttractionId == attractionId);
            if (favorite == null)
                return false;

            _unitOfWork.Favorites.Remove(favorite);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Attraction>> GetUserFavoritesAsync(int userId)
        {
            // 1. 先查出收藏记录
            var favorites = await _unitOfWork.Favorites.FindAsync(f => f.UserId == userId);
            
            // 2. 提取景点ID
            var attractionIds = favorites.Select(f => f.AttractionId).ToList();
            
            // 3. 逐个查询景点详情
            var attractions = new List<Attraction>();
            foreach (var id in attractionIds)
            {
                // 🔴🔴🔴 核心修复：加上关联查询参数 a => a.Images
                // 这样数据库才会把 Images 表的数据一起查出来
                var attraction = await _unitOfWork.Attractions.GetByIdAsync(id, 
                    a => a.Images,     // 加载图片
                    a => a.Category,   // 顺便加载分类
                    a => a.District    // 顺便加载区域
                );
                
                if (attraction != null)
                    attractions.Add(attraction);
            }

            // ================== 🔍 调试日志 ==================
            try
            {
                var debugOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string jsonString = JsonSerializer.Serialize(attractions, debugOptions);
                Console.WriteLine($"\n================= [Service] 收藏数据 (含Images) =================\n{jsonString}\n===========================================================\n");
            }
            catch (Exception ex) { Console.WriteLine($"JSON Log Error: {ex.Message}"); }
            // ===================================================
            
            return attractions;
        }

        public async Task<bool> IsFavoritedAsync(int userId, int attractionId)
        {
            return await _unitOfWork.Favorites.AnyAsync(f => f.UserId == userId && f.AttractionId == attractionId);
        }
    }
}
