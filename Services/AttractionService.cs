using Microsoft.EntityFrameworkCore;
using TourismPlatform.Models;
using TourismPlatform.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

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
                a => a.Images,     // ✅ 确保加载图片
                a => a.Category    // ✅ 确保加载分类
            );
        }

        // 🟢 修复 1：按地区筛选（加载图片）
        public async Task<IEnumerable<Attraction>> GetByDistrictAsync(int districtId)
        {
            // 旧代码（不加载关联数据）：
            // return await _unitOfWork.Attractions.FindAsync(a => a.DistrictId == districtId);

            // 新代码：先获取带关联数据的全集，再筛选
            var all = await _unitOfWork.Attractions.GetAllAsync(
                a => a.District,
                a => a.Images,
                a => a.Category
            );
            return all.Where(a => a.DistrictId == districtId);
        }

        // 🟢 修复 2：按分类筛选（加载图片）
        public async Task<IEnumerable<Attraction>> GetByCategoryAsync(int categoryId)
        {
            // 旧代码：
            // return await _unitOfWork.Attractions.FindAsync(a => a.CategoryId == categoryId);

            // 新代码：
            var all = await _unitOfWork.Attractions.GetAllAsync(
                a => a.District,
                a => a.Images,
                a => a.Category
            );
            return all.Where(a => a.CategoryId == categoryId);
        }

        // 🟢 修复 3：搜索功能（加载图片 + JSON 日志）
        public async Task<IEnumerable<Attraction>> SearchAsync(string keyword)
        {
            // 1. 获取带有 Images 的全量数据
            var all = await _unitOfWork.Attractions.GetAllAsync(
                a => a.District,
                a => a.Images,     // 关键：必须加载图片表
                a => a.Category
            );

            // 2. 在内存中进行模糊匹配筛选
            // (注意：如果数据量巨大，建议优化 Repository 支持 Where+Include，但目前对于旅游项目足够了)
            var result = all.Where(a => 
                (a.Name != null && a.Name.Contains(keyword)) || 
                (a.Description != null && a.Description.Contains(keyword))
            ).ToList();

            // ================== 🔍 调试日志 ==================
            try
            {
                var debugOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string jsonString = JsonSerializer.Serialize(result, debugOptions);
                Console.WriteLine($"\n================= [AttractionService] 搜索关键字: '{keyword}' (找到 {result.Count} 条) =================\n{jsonString}\n=========================================================================================\n");
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"JSON Log Error: {ex.Message}"); 
            }
            // ===================================================

            return result;
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
