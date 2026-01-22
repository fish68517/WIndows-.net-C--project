# 旅游平台 (Tourism Platform)

一个功能完整的 ASP.NET Core 旅游信息管理和预订平台，提供景点浏览、门票预订、游记分享、AI 助手等功能。

## 🌟 主要特性

### 用户功能
- ✅ 用户注册和登录
- ✅ 个人资料管理
- ✅ 景点浏览和搜索
- ✅ 门票预订和支付
- ✅ 酒店预订
- ✅ 游记发布和分享
- ✅ 评论和评分
- ✅ 收藏功能
- ✅ 个性化推荐
- ✅ 天气预报
- ✅ AI 对话助手

### 管理功能
- ✅ 景点管理 (CRUD)
- ✅ 酒店管理 (CRUD)
- ✅ 美食管理 (CRUD)
- ✅ 旅游线路管理
- ✅ 用户管理
- ✅ 订单管理
- ✅ 订单核销
- ✅ 评论审核
- ✅ 游记审核
- ✅ 数据统计
- ✅ 公告管理
- ✅ Excel 导出

## 🚀 快速开始

### 前置条件
- .NET 8.0 SDK
- SQL Server 或 LocalDB
- Entity Framework Core Tools

### 5 分钟快速启动

```bash
# 1. 恢复 NuGet 包
dotnet restore

# 2. 创建并应用数据库迁移
dotnet ef migrations add InitialCreate
dotnet ef database update

# 3. 运行应用
dotnet run
```

访问应用：
- **前台:** https://localhost:5001
- **后台:** https://localhost:5001/Admin/AdminAccount/Login
- **管理员账号:** admin / admin123

## 📚 文档

### 快速参考
- [快速开始指南](./QUICK_START.md) - 5 分钟快速启动
- [完整运行指南](./RUN_GUIDE.md) - 详细的运行和配置指南
- [安装指南](./INSTALLATION_GUIDE.md) - 不同操作系统的安装步骤
- [配置参考指南](./CONFIGURATION_REFERENCE.md) - 应用配置详解
- [文档索引](./DOCUMENTATION_INDEX.md) - 所有文档的索引

### 项目文档
- [需求文档](./.kiro/specs/tourism-platform/requirements.md) - 功能需求
- [设计文档](./.kiro/specs/tourism-platform/design.md) - 系统设计
- [集成验证报告](./.kiro/specs/tourism-platform/INTEGRATION_VERIFICATION.md) - 集成状态

## 🏗️ 项目结构

```
TourismPlatform/
├── Controllers/                    # 前台控制器 (16个)
├── Areas/Admin/Controllers/        # 后台管理控制器 (14个)
├── Models/                         # 数据模型 (19个)
├── Services/                       # 业务逻辑层 (13个服务)
├── Repositories/                   # 数据访问层
├── Views/                          # 前台视图 (16个目录)
├── Areas/Admin/Views/              # 后台视图 (15个目录)
├── Data/                           # 数据库上下文和迁移
├── wwwroot/                        # 静态资源 (CSS, JS, 图片)
├── appsettings.json                # 应用配置
├── TourismPlatform.csproj          # 项目文件
└── README.md                       # 本文件
```

## 🛠️ 技术栈

### 后端
- **框架:** ASP.NET Core 8.0
- **数据库:** SQL Server 2019+
- **ORM:** Entity Framework Core 8.0
- **认证:** ASP.NET Core Identity
- **缓存:** Memory Cache

### 前端
- **模板引擎:** Razor
- **样式:** Bootstrap 5.3
- **脚本:** Vanilla JavaScript
- **HTTP 客户端:** Fetch API

### 工具和库
- **Excel 导出:** EPPlus 7.0
- **密码加密:** BCrypt.Net-Next 4.0

## 📋 功能模块

### 1. 用户认证与账户管理
- 用户注册和登录
- 个人资料编辑
- 头像上传
- 密码修改

### 2. 景点信息浏览
- 景点列表展示
- 景点搜索和筛选
- 景点详情展示
- 周边美食和酒店推荐

### 3. 订单管理
- 门票订单创建和支付
- 酒店订单创建和支付
- 订单详情查看
- 订单历史记录

### 4. 社区功能
- 游记发布和编辑
- 评论和评分
- 收藏功能
- 用户互动

### 5. 个性化推荐
- 基于用户行为的推荐
- 热门景点推荐
- 相关景点推荐

### 6. 实用工具
- 天气预报
- AI 对话助手

### 7. 后台管理
- 内容管理 (景点、酒店、美食)
- 用户管理
- 订单管理
- 数据统计
- 公告管理

## 🔧 配置

### 数据库连接

编辑 `appsettings.json` 中的连接字符串：

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourismPlatformDb;Trusted_Connection=true;"
}
```

### 外部 API

配置天气 API (可选)：

```json
"ExternalApis": {
  "WeatherApi": {
    "BaseUrl": "https://api.weatherapi.com/v1",
    "ApiKey": "your-weather-api-key"
  }
}
```

### 文件上传

配置文件上传设置：

```json
"FileUpload": {
  "MaxFileSizeBytes": 5242880,
  "AllowedExtensions": ".jpg,.jpeg,.png,.gif",
  "UploadPath": "uploads"
}
```

更多配置选项，请查看 [配置参考指南](./CONFIGURATION_REFERENCE.md)。

## 🗄️ 数据库

### 初始化数据库

```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

### 种子数据

数据库初始化时会自动创建：
- 默认管理员账户 (admin / admin123)
- 6 个行政区
- 5 个景点类型
- 初始景点数据

### 数据库表

- **用户:** Users, AdminUsers
- **内容:** Attractions, AttractionImages, Foods, Hotels, HotelRoomTypes
- **订单:** TicketOrders, HotelOrders, VerifyCodes
- **社区:** Comments, Ratings, TravelDiaries, Favorites
- **管理:** TravelRoutes, Announcements

## 🚀 部署

### 发布应用

```bash
dotnet publish -c Release -o ./publish
```

### Docker 部署

```bash
# 构建镜像
docker build -t tourism-platform .

# 运行容器
docker run -p 5000:80 -p 5001:443 tourism-platform
```

### 生产环境配置

创建 `appsettings.Production.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_PRODUCTION_SERVER;Database=TourismPlatformDb;User Id=sa;Password=YOUR_PASSWORD;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com"
}
```

## 📊 API 端点

### 前台 API

| 方法 | 端点 | 说明 |
|------|------|------|
| GET | /Attractions | 景点列表 |
| GET | /Attractions/{id} | 景点详情 |
| GET | /Diaries | 游记列表 |
| POST | /Orders/CreateTicket | 创建门票订单 |
| POST | /Orders/CreateHotel | 创建酒店订单 |
| GET | /Me/Dashboard | 个人中心 |

### 后台 API

| 方法 | 端点 | 说明 |
|------|------|------|
| GET | /Admin/AdminAttractions | 景点管理 |
| POST | /Admin/AdminAttractions/Create | 创建景点 |
| GET | /Admin/AdminOrders | 订单管理 |
| POST | /Admin/AdminVerify/Verify | 订单核销 |
| GET | /Admin/AdminStats | 数据统计 |

## 🔐 安全性

- ✅ 密码加密 (BCrypt)
- ✅ HTTPS 支持
- ✅ CSRF 保护
- ✅ SQL 注入防护 (参数化查询)
- ✅ XSS 防护 (HTML 编码)
- ✅ 会话管理
- ✅ 访问控制

## 🧪 测试

### 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试
dotnet test --filter "TestClassName"
```

### 测试覆盖

- 单元测试: 业务逻辑层
- 集成测试: 数据访问层
- 端到端测试: 控制器和视图

## 📈 性能优化

- ✅ 内存缓存
- ✅ 数据库索引
- ✅ 异步操作
- ✅ 连接池
- ✅ 静态资源压缩

## 🐛 故障排除

### 常见问题

**Q: 无法连接到数据库**
A: 检查 SQL Server 是否运行，验证连接字符串

**Q: 迁移失败**
A: 删除迁移文件，重新创建迁移

**Q: 端口已被占用**
A: 修改 `appsettings.json` 中的端口配置

更多问题，请查看 [完整运行指南](./RUN_GUIDE.md) 的故障排除部分。

## 📞 获取帮助

- 📖 查看 [文档索引](./DOCUMENTATION_INDEX.md)
- 🔍 查看 [故障排除指南](./RUN_GUIDE.md#故障排除)
- 💬 查看 [常见问题](./QUICK_START.md#常见问题快速解答)

## 📝 许可证

本项目采用 MIT 许可证。详见 LICENSE 文件。

## 👥 贡献

欢迎提交 Issue 和 Pull Request！

## 📅 版本历史

### v1.0 (2024-01-21)
- ✅ 初始版本发布
- ✅ 完整的前台功能
- ✅ 完整的后台管理
- ✅ 数据库集成
- ✅ 完整的文档

## 🎯 未来计划

- [ ] 移动应用 (iOS/Android)
- [ ] 支付网关集成 (支付宝、微信)
- [ ] 实时通知系统
- [ ] 高级分析和报告
- [ ] 多语言支持
- [ ] API 文档 (Swagger)

## 📞 联系方式

- 📧 Email: support@tourism-platform.com
- 🌐 网站: https://tourism-platform.com
- 💬 讨论: GitHub Discussions

## 🙏 致谢

感谢所有贡献者和用户的支持！

---

## 🚀 立即开始

1. **快速启动:** [快速开始指南](./QUICK_START.md)
2. **详细安装:** [安装指南](./INSTALLATION_GUIDE.md)
3. **配置应用:** [配置参考指南](./CONFIGURATION_REFERENCE.md)
4. **查看文档:** [文档索引](./DOCUMENTATION_INDEX.md)

---

**祝你使用愉快！** 🎉

**最后更新:** 2024年1月21日  
**.NET 版本:** 8.0  
**数据库:** SQL Server 2019+

