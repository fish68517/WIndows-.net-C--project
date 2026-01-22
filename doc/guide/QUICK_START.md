# 旅游平台 - 快速开始指南

## 5 分钟快速启动

### 前置条件
- 已安装 .NET 8.0 SDK
- 已安装 SQL Server 或 LocalDB
- 已安装 Entity Framework Core 工具

### 快速步骤

#### 1. 恢复 NuGet 包
```bash
dotnet restore
```

#### 2. 创建并应用数据库迁移
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 3. 运行应用
```bash
dotnet run
```

#### 4. 访问应用
- **前台**: https://localhost:5001
- **后台**: https://localhost:5001/Admin/AdminAccount/Login
- **管理员账号**: admin / admin123

---

## 常用命令

### 数据库操作
```bash
# 创建迁移
dotnet ef migrations add MigrationName

# 应用迁移
dotnet ef database update

# 回滚迁移
dotnet ef database update PreviousMigrationName

# 删除最后一个迁移
dotnet ef migrations remove

# 查看迁移列表
dotnet ef migrations list
```

### 项目运行
```bash
# 运行项目
dotnet run

# 构建项目
dotnet build

# 发布项目
dotnet publish -c Release

# 清理项目
dotnet clean
```

### 包管理
```bash
# 恢复包
dotnet restore

# 清除 NuGet 缓存
dotnet nuget locals all --clear

# 添加包
dotnet add package PackageName

# 移除包
dotnet remove package PackageName
```

---

## 首次使用检查清单

- [ ] 已安装 .NET 8.0 SDK
- [ ] 已安装 SQL Server 或 LocalDB
- [ ] 已安装 Entity Framework Core 工具
- [ ] 已克隆或下载项目
- [ ] 已恢复 NuGet 包
- [ ] 已创建数据库迁移
- [ ] 已应用数据库迁移
- [ ] 已成功运行应用
- [ ] 已访问前台首页
- [ ] 已登录后台管理系统

---

## 主要功能入口

### 前台功能
| 功能 | URL | 说明 |
|------|-----|------|
| 首页 | / | 查看公告和推荐 |
| 景点浏览 | /Attractions | 浏览所有景点 |
| 游记 | /Diaries | 查看和发布游记 |
| 天气 | /Weather/Forecast | 天气预报 |
| AI助手 | /Chat | AI对话 |
| 个人中心 | /Me/Dashboard | 个人信息和订单 |
| 登录 | /Account/Login | 用户登录 |
| 注册 | /Account/Register | 用户注册 |

### 后台功能
| 功能 | URL | 说明 |
|------|-----|------|
| 管理员登录 | /Admin/AdminAccount/Login | 管理员登录 |
| 后台首页 | /Admin/AdminHome/Index | 数据概览 |
| 景点管理 | /Admin/AdminAttractions/Index | 景点CRUD |
| 酒店管理 | /Admin/AdminHotels/Index | 酒店CRUD |
| 美食管理 | /Admin/AdminFoods/Index | 美食CRUD |
| 线路管理 | /Admin/AdminRoutes/Index | 线路CRUD |
| 用户管理 | /Admin/AdminUsers/Index | 用户管理 |
| 订单管理 | /Admin/AdminOrders/Index | 订单管理 |
| 核销管理 | /Admin/AdminVerify/Index | 订单核销 |
| 评论审核 | /Admin/AdminComments/Index | 评论审核 |
| 游记审核 | /Admin/AdminDiaries/Index | 游记审核 |
| 数据统计 | /Admin/AdminStats/Index | 数据统计 |
| 公告管理 | /Admin/AdminAnnouncements/Index | 公告CRUD |

---

## 默认账户

### 管理员账户
- **用户名**: admin
- **密码**: admin123

### 测试用户
可以通过前台注册功能创建新用户账户进行测试。

---

## 配置文件位置

- **主配置**: `appsettings.json`
- **开发配置**: `appsettings.Development.json`
- **项目文件**: `TourismPlatform.csproj`
- **数据库上下文**: `Data/MyDbContext.cs`

---

## 项目结构

```
TourismPlatform/
├── Controllers/                    # 前台控制器
├── Areas/Admin/Controllers/        # 后台控制器
├── Models/                         # 数据模型
├── Services/                       # 业务逻辑
├── Repositories/                   # 数据访问
├── Views/                          # 前台视图
├── Areas/Admin/Views/              # 后台视图
├── Data/                           # 数据库
├── wwwroot/                        # 静态资源
├── appsettings.json                # 配置文件
└── TourismPlatform.csproj          # 项目文件
```

---

## 常见问题快速解答

**Q: 如何修改数据库连接？**
A: 编辑 `appsettings.json` 中的 `ConnectionStrings.DefaultConnection`

**Q: 如何修改应用端口？**
A: 在 `appsettings.json` 中配置 `Kestrel` 部分

**Q: 如何重置数据库？**
A: 删除数据库后重新运行 `dotnet ef database update`

**Q: 如何添加新的管理员？**
A: 在数据库 `AdminUsers` 表中插入新记录

**Q: 如何启用调试模式？**
A: 在 `appsettings.Development.json` 中修改日志级别为 `Debug`

---

## 下一步

- 查看完整的 [运行指南](./RUN_GUIDE.md)
- 查看 [需求文档](./requirements.md)
- 查看 [设计文档](./design.md)
- 查看 [集成验证报告](./.kiro/specs/tourism-platform/INTEGRATION_VERIFICATION.md)

---

**准备好了吗？开始运行应用吧！** 🚀

