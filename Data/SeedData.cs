using TourismPlatform.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TourismPlatform.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MyDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<MyDbContext>>()))
            {
                // 1. 确保数据库已存在
                context.Database.EnsureCreated();

                // ==========================================
                // 2. 初始化管理员 (AdminUser)
                // ==========================================
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

                // ==========================================
                // 3. 初始化行政区 (Districts)
                // ==========================================
                if (!context.Districts.Any())
                {
                    var districts = new List<District>
                    {
                        new District { Name = "市中区", Description = "济南市中心区域，包含泉城广场、趵突泉等主要景点" },
                        new District { Name = "历下区", Description = "历史文化区，包含千佛山、大明湖等景点" },
                        new District { Name = "槐荫区", Description = "西部区域，包含黄河风景区等景点" },
                        new District { Name = "天桥区", Description = "北部区域，包含泉城欧乐堡等景点" },
                        new District { Name = "历城区", Description = "东部区域，包含灵岩寺、九如山等景点" },
                        new District { Name = "长清区", Description = "南郊区域，包含五峰山、园博园等景点" }
                    };
                    context.Districts.AddRange(districts);
                    context.SaveChanges();
                }

                // ==========================================
                // 4. 初始化景点分类 (AttractionCategories)
                // ==========================================
                if (!context.AttractionCategories.Any())
                {
                    var categories = new List<AttractionCategory>
                    {
                        new AttractionCategory { Name = "历史遗迹", Description = "古代建筑、遗址、文物等历史文化景点" },
                        new AttractionCategory { Name = "自然风景", Description = "山水风景、自然保护区、生态景区等" },
                        new AttractionCategory { Name = "文化场所", Description = "博物馆、美术馆、文化中心等文化设施" },
                        new AttractionCategory { Name = "主题公园", Description = "游乐园、主题乐园、动物园等娱乐设施" },
                        new AttractionCategory { Name = "宗教场所", Description = "寺庙、道观、教堂等宗教建筑" },
                        new AttractionCategory { Name = "现代景观", Description = "现代建筑、城市公园、商业街区等现代景观" }
                    };
                    context.AttractionCategories.AddRange(categories);
                    context.SaveChanges();
                }

                // 获取外键关联数据
                var dbDistricts = context.Districts.ToList();
                var dbCategories = context.AttractionCategories.ToList();

                // 辅助方法
                int GetDistrictId(string name) => dbDistricts.FirstOrDefault(d => d.Name == name)?.DistrictId ?? dbDistricts.First().DistrictId;
                int GetCategoryId(string name) => dbCategories.FirstOrDefault(c => c.Name == name)?.CategoryId ?? dbCategories.First().CategoryId;

                // ==========================================
                // 5. 初始化景点 (Attractions)
                // ==========================================
                if (!context.Attractions.Any())
                {
                    var attractions = new List<Attraction>
                    {
                        new Attraction
                        {
                            Name = "趵突泉",
                            Description = "“天下第一泉”，济南七十二名泉之首。泉水清冽，三窟并发，声如隐雷。公园内亭台楼阁，古迹众多，是济南的象征。",
                            Address = "济南市市中区趵突泉南路1号",
                            TicketPrice = 40.00m,
                            OpeningHours = "07:00 - 19:00",
                            TransportInfo = "公交：乘坐1、5、41、66、82、85、101路公交车至趵突泉南门站下车。",
                            ContactPhone = "0531-86920680",
                            DistrictId = GetDistrictId("市中区"),
                            CategoryId = GetCategoryId("自然风景"),
                            ViewCount = 1024,
                            // 利用关联表添加图片 (因为Attractions表本身没图片字段)
                            Images = new List<AttractionImage>
                            {
                                new AttractionImage { ImageUrl = "https://images.unsplash.com/photo-1548013146-72479768bada?auto=format&fit=crop&w=800&q=80", DisplayOrder = 1 }
                            },
                            CreatedAt = DateTime.UtcNow
                        },
                        new Attraction
                        {
                            Name = "大明湖",
                            Description = "济南三大名胜之一，繁华都市中难得的天然湖泊。湖水来源于城内诸泉，有“四面荷花三面柳，一城山色半城湖”的美誉。",
                            Address = "济南市历下区大明湖路271号",
                            TicketPrice = 30.00m,
                            OpeningHours = "全天开放",
                            TransportInfo = "公交：乘坐11、41、K54、K95、K98、K109路公交车可达。",
                            ContactPhone = "0531-86088910",
                            DistrictId = GetDistrictId("历下区"),
                            CategoryId = GetCategoryId("自然风景"),
                            ViewCount = 899,
                            Images = new List<AttractionImage>
                            {
                                new AttractionImage { ImageUrl = "https://images.unsplash.com/photo-1528164344705-475426879fdc?auto=format&fit=crop&w=800&q=80", DisplayOrder = 1 }
                            },
                            CreatedAt = DateTime.UtcNow
                        },
                        new Attraction
                        {
                            Name = "千佛山",
                            Description = "古称历山，相传虞舜曾于山下耕田。山上有兴国禅寺，多处摩崖石刻，登顶可俯瞰济南全城。",
                            Address = "济南市历下区经十一路18号",
                            TicketPrice = 30.00m,
                            OpeningHours = "06:30 - 18:30",
                            TransportInfo = "公交：乘坐K2、K48、K51、K56、K68路至千佛山站。",
                            ContactPhone = "0531-82662292",
                            DistrictId = GetDistrictId("历下区"),
                            CategoryId = GetCategoryId("宗教场所"),
                            ViewCount = 560,
                            Images = new List<AttractionImage>
                            {
                                new AttractionImage { ImageUrl = "https://images.unsplash.com/photo-1599579086708-364230d52462?auto=format&fit=crop&w=800&q=80", DisplayOrder = 1 }
                            },
                            CreatedAt = DateTime.UtcNow
                        },
                        new Attraction
                        {
                            Name = "方特东方神画",
                            Description = "以非物质文化遗产为核心结合现代高科技的大型主题乐园。包含《牛郎织女》、《烈焰风云》等大型室内项目。",
                            Address = "济南市槐荫区济齐路277号",
                            TicketPrice = 280.00m,
                            OpeningHours = "09:30 - 17:30",
                            TransportInfo = "建议自驾或乘坐方特专线巴士。",
                            ContactPhone = "400-166-0006",
                            DistrictId = GetDistrictId("槐荫区"),
                            CategoryId = GetCategoryId("主题公园"),
                            ViewCount = 1200,
                            Images = new List<AttractionImage>
                            {
                                new AttractionImage { ImageUrl = "https://images.unsplash.com/photo-1513883049090-d0b7439799bf?auto=format&fit=crop&w=800&q=80", DisplayOrder = 1 }
                            },
                            CreatedAt = DateTime.UtcNow
                        }
                    };
                    context.Attractions.AddRange(attractions);
                    context.SaveChanges();
                }

                // ==========================================
                // 6. 初始化美食 (Foods)
                // ==========================================
                if (!context.Foods.Any())
                {
                    var baotuquan = context.Attractions.FirstOrDefault(a => a.Name == "趵突泉");
                    var daminghu = context.Attractions.FirstOrDefault(a => a.Name == "大明湖");

                    var foods = new List<Food>
                    {
                        new Food
                        {
                            Name = "把子肉",
                            Description = "济南传统名吃，肥而不腻，瘦而不柴。地址：济南市各大街巷。",
                            ReferencePrice = 18.00m,
                            ContactPhone = "暂无",
                            DistrictId = GetDistrictId("市中区"),
                            AttractionId = baotuquan?.AttractionId,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Food
                        {
                            Name = "甜沫",
                            Description = "虽叫甜沫，其实是咸的。一种以小米面为主熬制的咸粥。地址：大明湖附近早餐摊。",
                            ReferencePrice = 5.00m,
                            ContactPhone = "暂无",
                            DistrictId = GetDistrictId("历下区"),
                            AttractionId = daminghu?.AttractionId,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Food
                        {
                            Name = "九转大肠",
                            Description = "鲁菜代表作。色泽红润，质地软嫩，兼有酸、甜、香、辣、咸五味。地址：老字号鲁菜馆。",
                            ReferencePrice = 98.00m,
                            ContactPhone = "0531-88888888",
                            DistrictId = GetDistrictId("市中区"),
                            CreatedAt = DateTime.UtcNow
                        },
                        new Food
                        {
                            Name = "奶汤蒲菜",
                            Description = "济南汤菜之冠。汤呈乳白色，蒲菜脆嫩鲜香。地址：泉城路商圈。",
                            ReferencePrice = 58.00m,
                            ContactPhone = "0531-66666666",
                            DistrictId = GetDistrictId("历下区"),
                            AttractionId = daminghu?.AttractionId,
                            CreatedAt = DateTime.UtcNow
                        }
                    };
                    context.Foods.AddRange(foods);
                    context.SaveChanges();
                }

                // ==========================================
                // 7. 初始化酒店 (Hotels)
                // ==========================================
                if (!context.Hotels.Any())
                {
                    var hotels = new List<Hotel>
                    {
                        new Hotel
                        {
                            Name = "索菲特银座大饭店",
                            Description = "豪华五星体验，位于市中心，紧邻泉城广场和趵突泉。",
                            Address = "济南市历下区泺源大街66号",
                            ContactPhone = "0531-86068888",
                            StarRating = 5,
                            ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=800&q=80",
                            DistrictId = GetDistrictId("历下区"),
                            CreatedAt = DateTime.UtcNow
                        },
                        new Hotel
                        {
                            Name = "香格里拉大酒店",
                            Description = "坐拥泉城广场核心位置，可俯瞰大明湖和趵突泉美景。",
                            Address = "济南市历下区泺源大街106号",
                            ContactPhone = "0531-55575999",
                            StarRating = 5,
                            ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=800&q=80",
                            DistrictId = GetDistrictId("历下区"),
                            CreatedAt = DateTime.UtcNow
                        },
                        new Hotel
                        {
                            Name = "全季酒店（泉城广场店）",
                            Description = "舒适型连锁酒店，性价比高，交通便利。",
                            Address = "济南市市中区趵突泉北路",
                            ContactPhone = "0531-88886666",
                            StarRating = 3,
                            ImageUrl = "https://images.unsplash.com/photo-1618773928121-c32242e63f39?auto=format&fit=crop&w=800&q=80",
                            DistrictId = GetDistrictId("市中区"),
                            CreatedAt = DateTime.UtcNow
                        },
                        new Hotel
                        {
                            Name = "云止民宿",
                            Description = "位于九如山风景区内的精品民宿，亲近自然。",
                            Address = "济南市历城区九如山景区",
                            ContactPhone = "13800138000",
                            StarRating = 4,
                            ImageUrl = "https://images.unsplash.com/photo-1587061949409-02df41d5e562?auto=format&fit=crop&w=800&q=80",
                            DistrictId = GetDistrictId("历城区"),
                            CreatedAt = DateTime.UtcNow
                        }
                    };
                    context.Hotels.AddRange(hotels);
                    context.SaveChanges();
                }

                // ==========================================
                // 8. 初始化房型 (HotelRoomTypes)
                // ==========================================
                if (!context.HotelRoomTypes.Any())
                {
                    var dbHotels = context.Hotels.ToList();
                    var roomTypes = new List<HotelRoomType>();

                    foreach (var hotel in dbHotels)
                    {
                        // 基础房型
                        roomTypes.Add(new HotelRoomType 
                        { 
                            HotelId = hotel.HotelId, 
                            RoomTypeName = "标准大床房", 
                            PricePerNight = 350.00m + (hotel.StarRating * 50), 
                            Capacity = 2, 
                            Description = "25平米，大床1.8米，含双早" 
                        });

                        // 豪华房型
                        roomTypes.Add(new HotelRoomType 
                        { 
                            HotelId = hotel.HotelId, 
                            RoomTypeName = "豪华双床房", 
                            PricePerNight = 480.00m + (hotel.StarRating * 60), 
                            Capacity = 2, 
                            Description = "30平米，双床1.2米，观景落地窗" 
                        });
                    }
                    context.HotelRoomTypes.AddRange(roomTypes);
                    context.SaveChanges();
                }

                // ==========================================
                // 9. 初始化旅游路线 (TravelRoutes)
                // ==========================================
                if (!context.TravelRoutes.Any())
                {
                    var routes = new List<TravelRoute>
                    {
                        new TravelRoute 
                        { 
                            Name = "济南经典一日游", 
                            Description = "一日游遍济南三大名胜，人均约200元。", 
                            Itinerary = "上午：游览趵突泉 -> 中午：芙蓉街品尝小吃 -> 下午：大明湖泛舟 -> 晚上：宽厚里散步。", 
                            AttractionIntroduction = "涵盖趵突泉、大明湖、黑虎泉等核心景点。",
                            TransportSuggestion = "全程建议公交或步行，景点距离较近。",
                            CreatedAt = DateTime.UtcNow 
                        },
                        new TravelRoute 
                        { 
                            Name = "山水圣人三日游", 
                            Description = "济南+泰山+曲阜，深度体验齐鲁文化。", 
                            Itinerary = "Day1: 济南大明湖、千佛山；Day2: 登泰山观日出；Day3: 曲阜三孔。", 
                            AttractionIntroduction = "游览五岳之首泰山及孔子故里。",
                            TransportSuggestion = "城市间建议乘坐高铁。",
                            CreatedAt = DateTime.UtcNow 
                        }
                    };
                    context.TravelRoutes.AddRange(routes);
                    context.SaveChanges();
                }

                // ==========================================
                // 10. 初始化公告 (Announcements)
                // ==========================================
                if (!context.Announcements.Any())
                {
                    var announcements = new List<Announcement>
                    {
                        new Announcement { Title = "关于五一假期景区限流的通知", Content = "为保障游客安全，五一期间各大景区实施预约限流措施，请提前在平台购票。", PublishedAt = DateTime.UtcNow.AddDays(-10) },
                        new Announcement { Title = "大明湖荷花节开幕啦", Content = "第30届荷花节将于7月1日盛大开幕，欢迎广大游客前来赏荷。", PublishedAt = DateTime.UtcNow.AddDays(-5) },
                        new Announcement { Title = "平台系统维护公告", Content = "我们将于本周四凌晨 02:00 进行系统升级，届时将暂停服务约 2 小时。", PublishedAt = DateTime.UtcNow.AddDays(-2) }
                    };
                    context.Announcements.AddRange(announcements);
                    context.SaveChanges();
                }

                // ==========================================
                // 11. 初始化演示用户 (User)
                // ==========================================
                if (!context.Users.Any())
                {
                    var demoUser = new User
                    {
                        Email = "demo@tourism.com",
                        // 核心修改：普通用户必须用 BCrypt 加密，否则 UserService 无法登录
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Nickname = "旅游体验官",
                        Bio = "热爱旅行，热爱生活。",
                        // AvatarUrl 为 nullable，不赋值不会报错
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.Users.Add(demoUser);
                    context.SaveChanges();
                }

                // ==========================================
                // 12. 初始化游记 (TravelDiaries)
                // ==========================================
                if (!context.TravelDiaries.Any())
                {
                    var user = context.Users.FirstOrDefault();
                    if (user != null)
                    {
                        var diaries = new List<TravelDiary>
                        {
                            new TravelDiary 
                            { 
                                Title = "错过了四月天，别再错过济南的夏天", 
                                Content = "济南的夏天虽然热，但是泉水是凉的！趵突泉的泉水清澈见底，喝一口甘甜解渴...", 
                                UserId = user.UserId, 
                                CreatedAt = DateTime.UtcNow.AddDays(-5) 
                            },
                            new TravelDiary 
                            { 
                                Title = "方特一日游超全攻略", 
                                Content = "早上9点入园，先去玩伴你飞翔，排队人最少。中午在园区内吃特色面条，下午看孟姜女...", 
                                UserId = user.UserId, 
                                CreatedAt = DateTime.UtcNow.AddDays(-3) 
                            }
                        };
                        context.TravelDiaries.AddRange(diaries);
                        context.SaveChanges();
                    }
                }
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