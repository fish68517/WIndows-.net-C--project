using TourismPlatform.Models;
using TourismPlatform.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TourismPlatform.Services
{
    public class RecommendService : IRecommendService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RecommendService> _logger;

        public RecommendService(IUnitOfWork unitOfWork, ILogger<RecommendService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Attraction>> GetHomeRecommendationsAsync(int userId)
        {
            _logger.LogInformation($"[推荐系统] 开始计算【首页推荐】，用户ID: {userId}");

            var allAttractions = await _unitOfWork.Attractions.GetAllAsync(a => a.Images);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning($"[推荐系统] 用户 {userId} 不存在，返回默认热门推荐。");
                return allAttractions.OrderByDescending(a => a.ViewCount).Take(6);
            }

            // =================================================================================
            // 【核心修复 1】添加 f => f.Attraction 参数，确保加载关联的景点数据
            // =================================================================================
            var userFavorites = await _unitOfWork.Favorites.GetAllAsync(f => f.Attraction);
            
            // 增加空值过滤，防止脏数据导致报错
            var userFavoriteAttractions = userFavorites
                .Where(f => f.UserId == userId && f.Attraction != null) 
                .Select(f => f.Attraction)
                .ToList();

            // 2. 获取用户已去过的景点 (订单)
            var userOrders = await _unitOfWork.TicketOrders.GetAllAsync();
            var visitedAttractionIds = userOrders
                .Where(o => o.UserId == userId && o.Status == TicketOrderStatus.Used)
                .Select(o => o.AttractionId)
                .ToHashSet();

            // 3. 提取特征
            var preferredCategories = userFavoriteAttractions.Select(a => a.CategoryId).Distinct().ToHashSet();
            var preferredDistricts = userFavoriteAttractions.Select(a => a.DistrictId).Distinct().ToHashSet();

            _logger.LogInformation($"[推荐系统] 用户特征提取完毕: " +
                                   $"收藏数={userFavoriteAttractions.Count}, " +
                                   $"偏好分类ID=[{string.Join(",", preferredCategories)}], " +
                                   $"偏好地区ID=[{string.Join(",", preferredDistricts)}], " +
                                   $"已去过ID=[{string.Join(",", visitedAttractionIds)}]");

            // 4. 计算得分
            var scoredCandidates = allAttractions
                .Where(a => !visitedAttractionIds.Contains(a.AttractionId)) // 过滤已去过的
                .Select(a => 
                {
                    int score = CalculateRecommendationScore(a, preferredCategories, preferredDistricts);
                    return new { Attraction = a, Score = score };
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Attraction.ViewCount)
                .Take(6)
                .ToList();

            // 5. 打印详情
            foreach (var item in scoredCandidates)
            {
                _logger.LogInformation($"[推荐结果] 景点: {item.Attraction.Name} | 得分: {item.Score}");
            }

            return scoredCandidates.Select(x => x.Attraction);
        }

        public async Task<IEnumerable<Attraction>> GetRelatedAttractionsAsync(int attractionId, int userId)
        {
            _logger.LogInformation($"[推荐系统] 开始计算【相关推荐/猜你喜欢】，当前景点ID: {attractionId}, 用户ID: {userId}");

            var currentAttraction = await _unitOfWork.Attractions.GetByIdAsync(attractionId);
            if (currentAttraction == null) return new List<Attraction>();

            // =================================================================================
            // 【核心修复 2】这里同样必须添加 f => f.Attraction，否则 f.Attraction 为 null
            // =================================================================================
            var userFavorites = await _unitOfWork.Favorites.GetAllAsync(f => f.Attraction);
            
            // 安全地提取分类ID
            var preferredCategories = userFavorites
                .Where(f => f.UserId == userId && f.Attraction != null) // 过滤掉 null
                .Select(f => f.Attraction.CategoryId) // 现在 f.Attraction 不会是 null 了
                .ToHashSet();

            _logger.LogInformation($"[推荐系统] 用户个性化偏好分类ID: [{string.Join(",", preferredCategories)}]");

            // var allAttractions = await _unitOfWork.Attractions.GetAllAsync();
            var allAttractions = await _unitOfWork.Attractions.GetAllAsync(a => a.Images);
            
            // 2. 打分
            var scoredCandidates = allAttractions
                .Where(a => a.AttractionId != attractionId)
                .Select(a => 
                {
                    int score = 0;
                    var debugReason = new List<string>();

                    // 规则1: 同分类 (+3分)
                    if (a.CategoryId == currentAttraction.CategoryId) { score += 3; debugReason.Add("同分类+3"); }
                    
                    // 规则2: 同地区 (+2分)
                    if (a.DistrictId == currentAttraction.DistrictId) { score += 2; debugReason.Add("同地区+2"); }

                    // 规则3: 命中用户收藏偏好 (+2分)
                    if (preferredCategories.Contains(a.CategoryId)) { score += 2; debugReason.Add("命中收藏+2"); }

                    // 规则4: 有热度 (+1分)
                    if (a.ViewCount > 0) { score += 1; debugReason.Add("有热度+1"); }

                    return new { Attraction = a, Score = score, Reasons = string.Join(",", debugReason) };
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Attraction.ViewCount)
                .Take(6)
                .ToList();

            // 3. 打印得分详情
            foreach (var item in scoredCandidates)
            {
                _logger.LogInformation($"[相关推荐] 景点: {item.Attraction.Name} | 得分: {item.Score} | 原因: {item.Reasons}");
            }

            return scoredCandidates.Select(x => x.Attraction);
        }

        private int CalculateRecommendationScore(Attraction attraction, HashSet<int> preferredCategories, HashSet<int> preferredDistricts)
        {
            int score = 0;

            if (preferredCategories.Contains(attraction.CategoryId)) score += 3;
            if (preferredDistricts.Contains(attraction.DistrictId)) score += 2;
            if (attraction.ViewCount > 0) score += 1;

            return score;
        }
    }
}