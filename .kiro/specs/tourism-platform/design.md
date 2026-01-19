# 设计文档 - 旅游平台

## 概述

本设计文档描述了旅游平台的整体架构、核心组件、数据模型和实现策略。系统采用 ASP.NET Core MVC 架构，分为前台用户模块和后台管理模块，使用 SQL Server 作为数据库，EF Core 作为 ORM 框架。

## 架构

### 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                    ASP.NET Core MVC 应用                     │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │   前台模块        │         │   后台管理模块    │          │
│  │  (Controllers)   │         │  (Areas/Admin)   │          │
│  └──────────────────┘         └──────────────────┘          │
│           │                            │                     │
│  ┌────────▼────────────────────────────▼──────────┐         │
│  │         业务逻辑层 (Services)                   │         │
│  │  - 用户服务、景点服务、订单服务等              │         │
│  └────────┬────────────────────────────┬──────────┘         │
│           │                            │                     │
│  ┌────────▼────────────────────────────▼──────────┐         │
│  │      数据访问层 (Repositories)                  │         │
│  │  - 通用仓储模式实现                            │         │
│  └────────┬────────────────────────────┬──────────┘         │
│           │                            │                     │
│  ┌────────▼────────────────────────────▼──────────┐         │
│  │      EF Core DbContext (MyDbContext)           │         │
│  └────────┬────────────────────────────┬──────────┘         │
│           │                            │                     │
│           └────────────────┬───────────┘                     │
│                            │                                 │
│                   ┌────────▼────────┐                        │
│                   │   SQL Server    │                        │
│                   │    数据库        │                        │
│                   └─────────────────┘                        │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 项目结构

```
TourismPlatform/
├── Controllers/                    # 前台控制器
│   ├── AccountController.cs        # 用户认证
│   ├── ProfileController.cs        # 个人资料
│   ├── AttractionsController.cs    # 景点浏览
│   ├── HotelsController.cs         # 酒店信息
│   ├── FoodsController.cs          # 美食信息
│   ├── OrdersController.cs         # 订单管理
│   ├── DiariesController.cs        # 游记管理
│   ├── CommentsController.cs       # 评论管理
│   ├── FavoritesController.cs      # 收藏管理
│   ├── WeatherController.cs        # 天气预报
│   ├── ChatController.cs           # AI 对话
│   ├── RecommendController.cs      # 推荐引擎
│   └── MeController.cs             # 个人中心
├── Areas/
│   └── Admin/
│       └── Controllers/            # 后台管理控制器
│           ├── AdminAccountController.cs
│           ├── AdminAttractionsController.cs
│           ├── AdminHotelsController.cs
│           ├── AdminFoodsController.cs
│           ├── AdminRoutesController.cs
│           ├── AdminUsersController.cs
│           ├── AdminOrdersController.cs
│           ├── AdminVerifyController.cs
│           ├── AdminStatsController.cs
│           └── AdminAnnouncementsController.cs
├── Models/                         # 数据模型
│   ├── User.cs
│   ├── AdminUser.cs
│   ├── Attraction.cs
│   ├── AttractionImage.cs
│   ├── District.cs
│   ├── AttractionCategory.cs
│   ├── Food.cs
│   ├── Hotel.cs
│   ├── HotelRoomType.cs
│   ├── HotelOrder.cs
│   ├── TicketOrder.cs
│   ├── VerifyCode.cs
│   ├── Favorite.cs
│   ├── Comment.cs
│   ├── Rating.cs
│   ├── TravelDiary.cs
│   ├── TravelRoute.cs
│   └── Announcement.cs
├── Views/                          # 前台视图
│   ├── Account/
│   ├── Profile/
│   ├── Attractions/
│   ├── Hotels/
│   ├── Foods/
│   ├── Orders/
│   ├── Diaries/
│   ├── Me/
│   ├── Weather/
│   ├── Chat/
│   └── Shared/
├── Areas/Admin/Views/              # 后台视图
│   ├── Attractions/
│   ├── Hotels/
│   ├── Foods/
│   ├── Routes/
│   ├── Users/
│   ├── Orders/
│   ├── Verify/
│   ├── Stats/
│   ├── Announcements/
│   └── Shared/
├── Services/                       # 业务逻辑层
│   ├── IUserService.cs
│   ├── UserService.cs
│   ├── IAttractionService.cs
│   ├── AttractionService.cs
│   ├── IOrderService.cs
│   ├── OrderService.cs
│   ├── ICommentService.cs
│   ├── CommentService.cs
│   ├── IDiaryService.cs
│   ├── DiaryService.cs
│   ├── IFavoriteService.cs
│   ├── FavoriteService.cs
│   ├── IRecommendService.cs
│   ├── RecommendService.cs
│   ├── IWeatherService.cs
│   ├── WeatherService.cs
│   ├── IChatService.cs
│   ├── ChatService.cs
│   └── IVerifyService.cs
│   └── VerifyService.cs
├── Repositories/                   # 数据访问层
│   ├── IRepository.cs
│   ├── Repository.cs
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
├── Data/                           # 数据库上下文
│   ├── MyDbContext.cs
│   └── Migrations/
├── wwwroot/                        # 静态资源
│   ├── css/
│   ├── js/
│   ├── images/
│   ├── uploads/                    # 用户上传文件
│   │   ├── avatars/
│   │   ├── attractions/
│   │   └── diaries/
│   └── lib/                        # Bootstrap5 等库
├── Startup.cs                      # 应用启动配置
└── Program.cs                      # 应用入口
```

## 组件与接口

### 1. 用户认证与授权

**组件**：AccountController, UserService, User Model

**职责**：
- 用户注册、登录、登出
- 密码重置
- 身份验证和授权

**关键接口**：
```csharp
public interface IUserService
{
    Task<User> RegisterAsync(string email, string password, string nickname);
    Task<User> LoginAsync(string email, string password);
    Task<bool> UpdateProfileAsync(int userId, string nickname, string bio, IFormFile avatar);
    Task<User> GetUserByIdAsync(int userId);
}
```

### 2. 景点管理服务

**组件**：AttractionsController, AttractionService, Attraction Model

**职责**：
- 景点信息的 CRUD 操作
- 景点搜索和筛选
- 景点详情展示

**关键接口**：
```csharp
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
}
```

### 3. 订单管理服务

**组件**：OrdersController, OrderService, TicketOrder/HotelOrder Models

**职责**：
- 订单创建和管理
- 订单状态流转
- 模拟支付处理
- 核销码生成

**关键接口**：
```csharp
public interface IOrderService
{
    // 门票订单
    Task<TicketOrder> CreateTicketOrderAsync(int userId, int attractionId, DateTime visitDate, int quantity);
    Task<bool> PayTicketOrderAsync(int orderId);
    Task<TicketOrder> GetTicketOrderAsync(int orderId);
    Task<IEnumerable<TicketOrder>> GetUserTicketOrdersAsync(int userId);
    
    // 酒店订单
    Task<HotelOrder> CreateHotelOrderAsync(int userId, int roomTypeId, DateTime checkIn, DateTime checkOut);
    Task<bool> PayHotelOrderAsync(int orderId);
    Task<HotelOrder> GetHotelOrderAsync(int orderId);
    Task<IEnumerable<HotelOrder>> GetUserHotelOrdersAsync(int userId);
}
```

### 4. 评论与评分服务

**组件**：CommentsController, CommentService, Comment Model

**职责**：
- 评论的创建、删除
- 评分管理
- 评论审核

**关键接口**：
```csharp
public interface ICommentService
{
    Task<Comment> CreateAsync(int userId, int attractionId, string content, int rating);
    Task<bool> DeleteAsync(int commentId);
    Task<IEnumerable<Comment>> GetAttractionCommentsAsync(int attractionId);
    Task<IEnumerable<Comment>> GetDiaryCommentsAsync(int diaryId);
}
```

### 5. 游记服务

**组件**：DiariesController, DiaryService, TravelDiary Model

**职责**：
- 游记的创建、编辑、删除
- 游记列表展示
- 游记内容管理

**关键接口**：
```csharp
public interface IDiaryService
{
    Task<TravelDiary> CreateAsync(int userId, string title, string content, List<IFormFile> images);
    Task<bool> UpdateAsync(int diaryId, string title, string content);
    Task<bool> DeleteAsync(int diaryId);
    Task<TravelDiary> GetByIdAsync(int diaryId);
    Task<IEnumerable<TravelDiary>> GetAllAsync();
    Task<IEnumerable<TravelDiary>> GetUserDiariesAsync(int userId);
}
```

### 6. 收藏服务

**组件**：FavoritesController, FavoriteService, Favorite Model

**职责**：
- 收藏的添加和删除
- 收藏列表管理

**关键接口**：
```csharp
public interface IFavoriteService
{
    Task<bool> AddAsync(int userId, int attractionId);
    Task<bool> RemoveAsync(int userId, int attractionId);
    Task<IEnumerable<Attraction>> GetUserFavoritesAsync(int userId);
    Task<bool> IsFavoritedAsync(int userId, int attractionId);
}
```

### 7. 推荐引擎服务

**组件**：RecommendController, RecommendService

**职责**：
- 基于浏览历史的推荐
- 基于收藏记录的推荐
- 热门景点推荐

**关键接口**：
```csharp
public interface IRecommendService
{
    Task<IEnumerable<Attraction>> GetHomeRecommendationsAsync(int userId);
    Task<IEnumerable<Attraction>> GetRelatedAttractionsAsync(int attractionId, int userId);
}
```

### 8. 天气服务

**组件**：WeatherController, WeatherService

**职责**：
- 调用济南市气象局 API
- 天气数据缓存

**关键接口**：
```csharp
public interface IWeatherService
{
    Task<WeatherForecast> GetForecastAsync();
}
```

### 9. AI 对话服务

**组件**：ChatController, ChatService

**职责**：
- FAQ 规则库管理
- 问题匹配和回复

**关键接口**：
```csharp
public interface IChatService
{
    Task<string> GetAnswerAsync(string question);
}
```

### 10. 核销服务

**组件**：VerifyController, VerifyService, VerifyCode Model

**职责**：
- 核销码生成
- 核销码验证
- 核销状态管理

**关键接口**：
```csharp
public interface IVerifyService
{
    Task<VerifyCode> GenerateCodeAsync(int orderId, string orderType);
    Task<bool> VerifyAsync(string code);
    Task<VerifyCode> GetCodeAsync(string code);
}
```

## 数据模型

### 核心实体关系图

```
User (用户)
├── TravelDiary (游记)
├── Comment (评论)
├── Rating (评分)
├── Favorite (收藏)
├── TicketOrder (门票订单)
│   ├── Attraction (景点)
│   └── VerifyCode (核销码)
└── HotelOrder (酒店订单)
    ├── HotelRoomType (房型)
    │   └── Hotel (酒店)
    └── VerifyCode (核销码)

Attraction (景点)
├── AttractionCategory (景点类型)
├── District (行政区)
├── AttractionImage (景点图片)
├── Food (美食)
├── Hotel (酒店)
├── Comment (评论)
└── Rating (评分)

Hotel (酒店)
├── District (行政区)
├── HotelRoomType (房型)
└── HotelOrder (订单)

TravelDiary (游记)
├── User (用户)
└── Comment (评论)
```

### 主要实体字段

**User**
- UserId (PK)
- Email (Unique)
- PasswordHash
- Nickname
- Bio
- AvatarUrl
- IsActive
- CreatedAt
- UpdatedAt

**Attraction**
- AttractionId (PK)
- Name
- Description
- DistrictId (FK)
- CategoryId (FK)
- TicketPrice
- OpeningHours
- Address
- TransportInfo
- ContactPhone
- ViewCount
- CreatedAt

**TicketOrder**
- TicketOrderId (PK)
- UserId (FK)
- AttractionId (FK)
- VisitDate
- Quantity
- TotalPrice
- Status (Enum: PendingPay, Paid, Cancelled, ToUse, Used)
- CreatedAt
- PaidAt

**HotelOrder**
- HotelOrderId (PK)
- UserId (FK)
- RoomTypeId (FK)
- CheckInDate
- CheckOutDate
- TotalPrice
- Status (Enum: PendingPay, Paid, Cancelled, ToUse, Used)
- CreatedAt
- PaidAt

**VerifyCode**
- VerifyCodeId (PK)
- Code (Unique)
- OrderId
- OrderType (Enum: Ticket, Hotel)
- Status (Enum: Unused, Used)
- UsedAt
- CreatedAt

**Comment**
- CommentId (PK)
- UserId (FK)
- AttractionId (FK, Nullable)
- DiaryId (FK, Nullable)
- Content
- Rating (1-5, Nullable)
- CreatedAt

**TravelDiary**
- DiaryId (PK)
- UserId (FK)
- Title
- Content
- CreatedAt
- UpdatedAt

**Favorite**
- FavoriteId (PK)
- UserId (FK)
- AttractionId (FK)
- CreatedAt

## 错误处理

### 异常处理策略

1. **业务逻辑异常**：
   - 订单状态非法转换 → 返回 400 Bad Request
   - 用户未授权操作 → 返回 403 Forbidden
   - 资源不存在 → 返回 404 Not Found

2. **数据库异常**：
   - 连接失败 → 返回 500 Internal Server Error，记录日志
   - 并发冲突 → 重试或返回冲突提示

3. **外部 API 异常**：
   - 天气 API 调用失败 → 返回缓存数据或默认值
   - 超时 → 返回友好错误提示

### 错误响应格式

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }
}
```

## 测试策略

### 单元测试

- 业务逻辑层（Services）的核心功能测试
- 数据验证测试
- 推荐算法测试

### 集成测试

- 订单流程端到端测试（创建 → 支付 → 核销）
- 用户认证流程测试
- 数据库操作测试

### 测试框架

- xUnit 或 NUnit
- Moq 用于 Mock 依赖
- 最小化测试覆盖，仅覆盖核心业务逻辑

## 关键设计决策

### 1. 订单状态管理

使用 Enum 定义订单状态，确保状态转换的合法性：
- TicketOrderStatus: PendingPay → Paid → ToUse → Used
- HotelOrderStatus: PendingPay → Paid → ToUse → Used/CheckedIn

### 2. 核销码生成

- 使用 GUID 或随机字符串生成唯一核销码
- 核销码与订单一一对应
- 核销后标记为已使用，防止重复核销

### 3. 推荐算法

基于以下规则进行加权排序：
1. 同类别景点优先（权重：3）
2. 同行政区景点加分（权重：2）
3. 热门度排序（权重：1）
   - 点击量
   - 收藏数
   - 订单数

### 4. 文件上传管理

- 用户头像存储在 `wwwroot/uploads/avatars/`
- 景点图片存储在 `wwwroot/uploads/attractions/`
- 游记图片存储在 `wwwroot/uploads/diaries/`
- 使用 GUID 重命名文件防止冲突

### 5. 缓存策略

- 景点列表缓存 1 小时
- 天气数据缓存 3 小时
- 热门景点排行缓存 6 小时

### 6. 安全性考虑

- 密码使用 bcrypt 加密存储
- 使用 ASP.NET Core Identity 进行身份验证
- 后台操作需要管理员角色验证
- 防止 SQL 注入（使用 EF Core 参数化查询）
- CSRF 保护（使用 ASP.NET Core 内置 CSRF Token）

## 部署与配置

### 数据库连接字符串

在 `Startup.cs` 中配置：
```csharp
services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
```

### 静态资源配置

在 `Startup.cs` 中启用静态文件服务：
```csharp
app.UseStaticFiles();
```

### 依赖注入配置

在 `Startup.cs` 中注册服务：
```csharp
services.AddScoped<IUserService, UserService>();
services.AddScoped<IAttractionService, AttractionService>();
// ... 其他服务
```
