using TourismPlatform.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // 可能也需要这个

namespace TourismPlatform.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MyDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<MyDbContext>>()))
            {
                // Check if data already exists
                if (context.Districts.Any() || context.AttractionCategories.Any())
                {
                    return;
                }

                // Seed default admin user if not exists
                if (!context.AdminUsers.Any())
                {
                    var adminUser = new AdminUser
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("admin123"),
                        Email = "admin@tourism.com",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.AdminUsers.Add(adminUser);
                    context.SaveChanges();
                }

                // Seed Districts (行政区)
                var districts = new List<District>
                {
                    new District
                    {
                        Name = "市中区",
                        Description = "济南市中心区域，包含泉城广场、趵突泉等主要景点"
                    },
                    new District
                    {
                        Name = "历下区",
                        Description = "历史文化区，包含千佛山、大明湖等景点"
                    },
                    new District
                    {
                        Name = "槐荫区",
                        Description = "西部区域，包含黄河风景区等景点"
                    },
                    new District
                    {
                        Name = "天桥区",
                        Description = "南部区域，包含泉城欧乐堡等景点"
                    },
                    new District
                    {
                        Name = "历城区",
                        Description = "东部区域，包含灵岩寺、九如山等景点"
                    },
                    new District
                    {
                        Name = "长清区",
                        Description = "南郊区域，包含五峰山、园博园等景点"
                    }
                };

                context.Districts.AddRange(districts);
                context.SaveChanges();

                // Seed Attraction Categories (景点类型)
                var categories = new List<AttractionCategory>
                {
                    new AttractionCategory
                    {
                        Name = "历史遗迹",
                        Description = "古代建筑、遗址、文物等历史文化景点"
                    },
                    new AttractionCategory
                    {
                        Name = "自然风景",
                        Description = "山水风景、自然保护区、生态景区等"
                    },
                    new AttractionCategory
                    {
                        Name = "文化场所",
                        Description = "博物馆、美术馆、文化中心等文化设施"
                    },
                    new AttractionCategory
                    {
                        Name = "主题公园",
                        Description = "游乐园、主题乐园、动物园等娱乐设施"
                    },
                    new AttractionCategory
                    {
                        Name = "宗教场所",
                        Description = "寺庙、道观、教堂等宗教建筑"
                    },
                    new AttractionCategory
                    {
                        Name = "现代景观",
                        Description = "现代建筑、城市公园、商业街区等现代景观"
                    }
                };

                context.AttractionCategories.AddRange(categories);
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
