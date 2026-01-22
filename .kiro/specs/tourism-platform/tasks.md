# 实现计划 - 旅游平台

## 项目初始化与基础设施

- [x] 1. 初始化 ASP.NET Core MVC 项目结构




  - 创建 Controllers、Models、Views、Services、Repositories、Data 目录
  - 配置 wwwroot 目录结构（css、js、images、uploads 子目录）
  - 配置 Areas/Admin 目录用于后台管理
  - _需求: 1.1, 2.1, 4.1_

- [x] 2. 配置 EF Core 和 SQL Server 数据库连接





  - 在 Startup.cs 中配置 DbContext 和连接字符串
  - 创建 MyDbContext 类并定义所有 DbSet
  - 执行初始迁移并创建数据库表
  - _需求: 1.1, 2.1_

- [x] 3. 创建所有数据模型和实体类





  - 创建 User、AdminUser、Attraction、AttractionImage、District、AttractionCategory 模型
  - 创建 Food、Hotel、HotelRoomType、HotelOrder、TicketOrder 模型
  - 创建 VerifyCode、Favorite、Comment、Rating、TravelDiary、TravelRoute、Announcement 模型
  - 定义模型之间的关系和约束
  - _需求: 1.1, 2.1, 3.1_

- [x] 4. 配置依赖注入和服务注册





  - 在 Startup.cs 中注册所有 Service 和 Repository
  - 配置 ASP.NET Core Identity 用于用户认证
  - 配置 CORS 和其他中间件
  - _需求: 1.1_

## 用户认证与账户管理

- [x] 5. 实现用户注册功能





  - 创建 AccountController 的 Register 方法
  - 创建 Register.cshtml 视图（邮箱、密码、昵称表单）
  - 实现 UserService 的 RegisterAsync 方法
  - 验证邮箱唯一性和密码强度
  - _需求: 1.1_

- [x] 6. 实现用户登录功能


















  - 创建 AccountController 的 Login 方法
  - 创建 Login.cshtml 视图
  - 实现 UserService 的 LoginAsync 方法
  - 使用 ASP.NET Core Identity 进行身份验证
  - _需求: 1.1_

- [x] 7. 实现个人资料管理





  - 创建 ProfileController 的 Index 和 Edit 方法
  - 创建 Profile/Index.cshtml 和 Edit.cshtml 视图
  - 实现 UserService 的 UpdateProfileAsync 方法
  - 实现头像上传功能（保存到 wwwroot/uploads/avatars/）
  - _需求: 1.1_

- [x] 8. 实现用户登出功能





  - 创建 AccountController 的 Logout 方法
  - 清除用户会话和认证 Cookie
  - _需求: 1.1_

## 景点信息管理与展示

- [x] 9. 创建景点相关的基础数据





  - 创建 District（行政区）数据
  - 创建 AttractionCategory（景点类型）数据
  - 在数据库中插入初始数据
  - _需求: 2.1, 2.2_

- [x] 10. 实现景点列表页面





  - 创建 AttractionsController 的 Index 方法
  - 创建 Attractions/Index.cshtml 视图
  - 实现 AttractionService 的 GetAllAsync 方法
  - 显示景点列表（名称、图片、票价、所属区域）
  - _需求: 2.1_

- [x] 11. 实现景点筛选功能





  - 在 AttractionsController 的 Index 方法中添加筛选参数
  - 实现 AttractionService 的 GetByDistrictAsync 和 GetByCategoryAsync 方法
  - 在视图中添加筛选表单（行政区、景点类型）
  - _需求: 2.1_

- [x] 12. 实现景点搜索功能




  - 在 AttractionsController 的 Index 方法中添加搜索参数
  - 实现 AttractionService 的 SearchAsync 方法
  - 在视图中添加搜索输入框
  - _需求: 2.1_

- [x] 13. 实现景点详情页面





  - 创建 AttractionsController 的 Detail 方法
  - 创建 Attractions/Detail.cshtml 视图
  - 实现 AttractionService 的 GetByIdAsync 方法
  - 显示景点详细信息（名称、相册、开放时间、票价、地址、交通、联系方式）
  - 增加景点浏览计数
  - _需求: 2.2_

- [x] 14. 在景点详情页显示周边美食





  - 创建 FoodsController 的 ListByAttraction 方法
  - 实现 FoodService 的 GetByAttractionAsync 方法
  - 在 Attractions/Detail.cshtml 中添加美食列表区域
  - 显示美食名称、参考价格、联系方式
  - _需求: 2.2_

- [x] 15. 在景点详情页显示周边酒店




  - 创建 HotelsController 的 ListByAttraction 方法
  - 实现 HotelService 的 GetByAttractionAsync 方法
  - 在 Attractions/Detail.cshtml 中添加酒店列表区域
  - 显示酒店名称、参考价格、联系方式
  - _需求: 2.2_

## 订单管理与支付

- [x] 16. 实现门票订单创建





  - 创建 OrdersController 的 CreateTicket 方法
  - 创建 Orders/CreateTicket.cshtml 视图（选择日期、数量）
  - 实现 OrderService 的 CreateTicketOrderAsync 方法
  - 验证订单参数并创建订单记录
  - _需求: 3.1_

- [x] 17. 实现门票订单模拟支付





  - 创建 OrdersController 的 PayTicket 方法
  - 创建 Orders/PayTicket.cshtml 视图（模拟支付表单）
  - 实现 OrderService 的 PayTicketOrderAsync 方法
  - 更新订单状态为"已支付"
  - _需求: 3.1_
-

- [x] 18. 实现酒店订单创建




  - 创建 OrdersController 的 CreateHotel 方法
  - 创建 Orders/CreateHotel.cshtml 视图（选择房型、入住/离店日期）
  - 实现 OrderService 的 CreateHotelOrderAsync 方法
  - 验证订单参数并创建订单记录
  - _需求: 3.2_

- [x] 19. 实现酒店订单模拟支付





  - 创建 OrdersController 的 PayHotel 方法
  - 创建 Orders/PayHotel.cshtml 视图（模拟支付表单）
  - 实现 OrderService 的 PayHotelOrderAsync 方法
  - 更新订单状态为"已支付"
  - _需求: 3.2_

- [x] 20. 实现核销码生成





  - 实现 VerifyService 的 GenerateCodeAsync 方法
  - 在订单支付成功后自动生成核销码
  - 使用 GUID 或随机字符串生成唯一核销码
  - _需求: 3.1, 3.2_

- [x] 21. 实现订单详情页面





  - 创建 OrdersController 的 Detail 方法
  - 创建 Orders/Detail.cshtml 视图
  - 显示订单信息、状态、核销码
  - _需求: 3.1, 3.2_

- [x] 22. 实现用户订单列表




  - 创建 OrdersController 的 List 方法
  - 创建 Orders/List.cshtml 视图
  - 实现 OrderService 的 GetUserTicketOrdersAsync 和 GetUserHotelOrdersAsync 方法
  - 显示用户的所有订单列表
  - _需求: 3.1, 3.2_

## 评论与评分

- [x] 23. 实现景点评论功能





  - 创建 CommentsController 的 Create 方法
  - 实现 CommentService 的 CreateAsync 方法
  - 仅允许订单状态为"已使用"的用户评论
  - 保存评论内容和评分（1-5 星）
  - _需求: 4.1_

- [x] 24. 实现景点评论列表展示





  - 在 Attractions/Detail.cshtml 中添加评论列表区域
  - 实现 CommentService 的 GetAttractionCommentsAsync 方法
  - 显示评论者昵称、评分、内容、发布时间
  - _需求: 4.1_

- [x] 25. 实现评论删除功能





  - 创建 CommentsController 的 Delete 方法
  - 实现 CommentService 的 DeleteAsync 方法
  - 仅允许评论作者或管理员删除评论
  - _需求: 4.1_

## 游记管理

- [x] 26. 实现游记发布功能




  - 创建 DiariesController 的 Create 方法
  - 创建 Diaries/Create.cshtml 视图（标题、内容编辑器、图片上传）
  - 实现 DiaryService 的 CreateAsync 方法
  - 保存游记内容并上传图片到 wwwroot/uploads/diaries/
  - _需求: 5.1_

- [x] 27. 实现游记列表页面





  - 创建 DiariesController 的 Index 方法
  - 创建 Diaries/Index.cshtml 视图
  - 实现 DiaryService 的 GetAllAsync 方法
  - 显示所有游记的标题、摘要、作者、发布时间
  - _需求: 5.1_

- [x] 28. 实现游记详情页面





  - 创建 DiariesController 的 Detail 方法
  - 创建 Diaries/Detail.cshtml 视图
  - 实现 DiaryService 的 GetByIdAsync 方法
  - 显示完整游记内容和相关图片
  - _需求: 5.1_

- [x] 29. 实现游记编辑功能




  - 创建 DiariesController 的 Edit 方法
  - 创建 Diaries/Edit.cshtml 视图
  - 实现 DiaryService 的 UpdateAsync 方法
  - 仅允许游记作者编辑
  - _需求: 5.1_

- [x] 30. 实现游记删除功能




  - 创建 DiariesController 的 Delete 方法
  - 实现 DiaryService 的 DeleteAsync 方法
  - 仅允许游记作者或管理员删除
  - _需求: 5.1_

- [x] 31. 实现游记评论功能





  - 在 Diaries/Detail.cshtml 中添加评论区域
  - 实现 CommentService 的 GetDiaryCommentsAsync 方法
  - 允许用户在游记下发表评论
  - _需求: 5.1_

## 收藏功能

- [x] 32. 实现景点收藏功能




  - 创建 FavoritesController 的 Add 和 Remove 方法
  - 实现 FavoriteService 的 AddAsync 和 RemoveAsync 方法
  - 在景点详情页添加"收藏"按钮
  - _需求: 5.2_

- [x] 33. 实现收藏列表页面





  - 创建 FavoritesController 的 List 方法
  - 创建 Favorites/List.cshtml 视图
  - 实现 FavoriteService 的 GetUserFavoritesAsync 方法
  - 显示用户收藏的所有景点
  - _需求: 5.2_

- [x] 34. 实现收藏状态检查





  - 实现 FavoriteService 的 IsFavoritedAsync 方法
  - 在景点详情页显示收藏状态（已收藏/未收藏）
  - _需求: 5.2_

## 个性化推荐

- [x] 35. 实现推荐算法





  - 实现 RecommendService 的 GetHomeRecommendationsAsync 方法
  - 基于用户浏览历史和收藏记录进行推荐
  - 实现加权排序：同类别（权重 3）、同区域（权重 2）、热门度（权重 1）
  - _需求: 6.1_

- [x] 36. 实现首页推荐展示





  - 创建 RecommendController 的 ForHome 方法
  - 在首页添加"猜你喜欢"区域
  - 调用推荐服务获取推荐景点
  - _需求: 6.1_

- [x] 37. 实现景点详情页相关推荐





  - 创建 RecommendController 的 ForAttraction 方法
  - 实现 RecommendService 的 GetRelatedAttractionsAsync 方法
  - 在景点详情页下方显示"相关推荐"
  - _需求: 6.1_

## 实用工具

- [x] 38. 实现天气预报功能





  - 创建 WeatherController 的 Forecast 方法
  - 创建 Weather/Forecast.cshtml 视图
  - 实现 WeatherService 的 GetForecastAsync 方法
  - 调用济南市气象局 API 获取 3-7 天天气数据
  - 实现天气数据缓存（3 小时）
  - _需求: 7.1_

- [x] 39. 实现 AI 对话功能





  - 创建 ChatController 的 Index 和 Ask 方法
  - 创建 Chat/Index.cshtml 视图（对话界面）
  - 实现 ChatService 的 GetAnswerAsync 方法
  - 创建 FAQ 规则库（常见问题和答案）
  - 实现问题匹配和回复逻辑
  - _需求: 7.2_

## 个人中心

- [x] 40. 实现个人中心首页





  - 创建 MeController 的 Dashboard 方法
  - 创建 Me/Dashboard.cshtml 视图
  - 显示用户基本信息和快速导航
  - _需求: 8.1_

- [x] 41. 实现"我的游记"管理





  - 创建 MeContentController 的 MyDiaries 方法
  - 创建 Me/MyDiaries.cshtml 视图
  - 实现 DiaryService 的 GetUserDiariesAsync 方法
  - 显示用户发布的所有游记，允许编辑和删除
  - _需求: 8.1_

- [x] 42. 实现"我的评论"管理




  - 创建 MeContentController 的 MyComments 方法
  - 创建 Me/MyComments.cshtml 视图
  - 显示用户发表的所有评论，允许删除
  - _需求: 8.1_

- [x] 43. 实现"我的收藏"管理




  - 创建 MeFavoritesController 的 Index 方法
  - 创建 Me/Favorites.cshtml 视图
  - 显示用户收藏的所有景点和酒店
  - _需求: 8.1_

- [x] 44. 实现"我的订单"管理





  - 创建 MeOrdersController 的 Tickets 和 Hotels 方法
  - 创建 Me/Tickets.cshtml 和 Me/Hotels.cshtml 视图
  - 实现订单列表展示和按状态筛选
  - _需求: 8.1_

## 后台管理 - 基础设施

- [x] 45. 实现管理员登录




  - 创建 Areas/Admin/Controllers/AdminAccountController
  - 创建 Areas/Admin/Views/Account/Login.cshtml
  - 实现管理员身份验证（预设账号）
  - _需求: 9.1_

- [x] 46. 创建后台管理首页




  - 创建 Areas/Admin/Controllers/AdminHomeController
  - 创建 Areas/Admin/Views/Home/Index.cshtml
  - 显示管理员导航菜单和快速统计
  - _需求: 9.1_

## 后台管理 - 内容管理

- [x] 47. 实现景点 CRUD 管理





  - 创建 Areas/Admin/Controllers/AdminAttractionsController
  - 创建 Areas/Admin/Views/Attractions/ 视图（Index、Create、Edit、Delete）
  - 实现景点的创建、编辑、删除功能
  - 支持景点图片上传到 wwwroot/uploads/attractions/
  - _需求: 9.2_

- [x] 48. 实现美食 CRUD 管理





  - 创建 Areas/Admin/Controllers/AdminFoodsController
  - 创建 Areas/Admin/Views/Foods/ 视图
  - 实现美食的创建、编辑、删除功能
  - _需求: 9.2_

- [x] 49. 实现酒店 CRUD 管理





  - 创建 Areas/Admin/Controllers/AdminHotelsController
  - 创建 Areas/Admin/Views/Hotels/ 视图
  - 实现酒店和房型的创建、编辑、删除功能
  - _需求: 9.2_

- [x] 50. 实现官方线路管理





  - 创建 Areas/Admin/Controllers/AdminRoutesController
  - 创建 Areas/Admin/Views/Routes/ 视图
  - 实现线路的创建、编辑、删除功能
  - 支持行程安排、景点介绍、交通建议等内容
  - _需求: 9.3_

## 后台管理 - 审核与用户管理

- [x] 51. 实现评论审核功能





  - 创建 Areas/Admin/Controllers/AdminCommentsController
  - 创建 Areas/Admin/Views/Comments/ 视图
  - 显示所有用户评论列表
  - 实现评论删除功能
  - _需求: 9.4_

- [x] 52. 实现游记审核功能





  - 创建 Areas/Admin/Controllers/AdminDiariesController
  - 创建 Areas/Admin/Views/Diaries/ 视图
  - 显示所有用户游记列表
  - 实现游记删除功能
  - _需求: 9.4_

- [x] 53. 实现用户管理功能




  - 创建 Areas/Admin/Controllers/AdminUsersController
  - 创建 Areas/Admin/Views/Users/ 视图
  - 显示用户列表（用户名、邮箱、注册时间、状态）
  - 实现按用户名和注册时间筛选
  - 实现用户禁用和启用功能
  - _需求: 9.5_

## 后台管理 - 订单与核销

- [x] 54. 实现订单管理功能





  - 创建 Areas/Admin/Controllers/AdminOrdersController
  - 创建 Areas/Admin/Views/Orders/ 视图
  - 显示门票订单和酒店订单列表
  - 实现按状态筛选订单
  - 实现订单状态更新和退款审核
  - _需求: 9.6_

- [x] 55. 实现订单 Excel 导出





  - 在 AdminOrdersController 中实现 ExportToExcel 方法
  - 支持按时间范围和景点/线路条件导出
  - 使用 EPPlus 或类似库生成 Excel 文件
  - _需求: 9.6_

- [x] 56. 实现核销管理功能





  - 创建 Areas/Admin/Controllers/AdminVerifyController
  - 创建 Areas/Admin/Views/Verify/ 视图
  - 显示待核销的订单列表
  - 实现核销码输入和验证
  - 实现核销操作（更新订单状态为"已使用"）
  - _需求: 3.3_

## 后台管理 - 数据统计

- [x] 57. 实现统计数据收集





  - 创建 Areas/Admin/Controllers/AdminStatsController
  - 实现统计服务获取关键指标
  - 计算总用户数、总景点数、总游记数、总订单数、总收入等
  - _需求: 9.7_

- [x] 58. 实现热门景点趋势图





  - 在 AdminStatsController 中实现图表数据接口
  - 使用 ASPChart 库显示热门景点趋势图
  - 按点击量、收藏数、订单数排序
  - 创建 Areas/Admin/Views/Stats/Index.cshtml 视图
  - _需求: 9.7_

## 后台管理 - 公告管理

- [x] 59. 实现公告管理功能




  - 创建 Areas/Admin/Controllers/AdminAnnouncementsController
  - 创建 Areas/Admin/Views/Announcements/ 视图
  - 实现公告的创建、编辑、删除功能
  - _需求: 9.8_

- [x] 60. 实现前台公告展示





  - 在前台首页显示最新公告
  - 创建公告详情页面
  - _需求: 9.8_

## 前端界面与样式

- [x] 61. 创建前台主布局和导航





  - 创建 Views/Shared/_Layout.cshtml
  - 实现响应式导航栏（使用 Bootstrap5）
  - 添加用户登录/登出链接
  - _需求: 1.1, 2.1_

- [x] 62. 创建后台主布局和导航




  - 创建 Areas/Admin/Views/Shared/_Layout.cshtml
  - 实现后台管理菜单
  - 添加管理员登出链接
  - _需求: 9.1_

- [x] 63. 创建前台通用样式





  - 在 wwwroot/css/ 中创建自定义样式文件
  - 实现响应式设计
  - 优化用户界面和交互体验
  - _需求: 2.1, 2.2_

- [x] 64. 创建后台通用样式




  - 在 wwwroot/css/ 中创建后台样式文件
  - 实现表格、表单、按钮等组件样式
  - _需求: 9.1_

## 前端交互脚本

- [x] 65. 实现前台交互脚本





  - 在 wwwroot/js/ 中创建前台脚本文件
  - 实现表单验证、动态加载、AJAX 请求等
  - _需求: 2.1, 3.1_

- [x] 66. 实现后台交互脚本





  - 在 wwwroot/js/ 中创建后台脚本文件
  - 实现表格操作、确认对话框、数据加载等
  - _需求: 9.1_

## 集成与测试

- [x] 67. 集成所有模块





  - 确保所有控制器、服务、视图正确连接
  - 验证依赖注入配置
  - 测试路由和导航
  - _需求: 1.1, 2.1, 3.1_

- [ ] 68. 端到端流程测试
  - 测试用户注册、登录、浏览景点、下单、支付、核销的完整流程
  - 测试游记发布、评论、收藏等社区功能
  - 测试后台管理功能
  - _需求: 1.1, 2.1, 3.1, 4.1, 5.1, 8.1, 9.1_

- [ ]* 69. 编写单元测试
  - 为 UserService、AttractionService、OrderService 等核心服务编写单元测试
  - 测试业务逻辑和数据验证
  - _需求: 1.1, 2.1, 3.1_

- [ ]* 70. 编写集成测试
  - 测试数据库操作和事务处理
  - 测试订单流程的完整性
  - _需求: 3.1, 3.2_

## 部署与优化

- [x] 71. 配置应用程序设置





  - 配置 appsettings.json（数据库连接、API 密钥等）
  - 配置日志记录
  - _需求: 1.1_

- [ ] 72. 实现错误处理和日志记录
  - 创建全局异常处理中间件
  - 实现日志记录功能
  - _需求: 1.1_

- [ ] 73. 性能优化
  - 实现数据库查询优化（使用 Include、Select 等）
  - 实现缓存策略（景点列表、天气数据等）
  - _需求: 2.1, 7.1_

- [ ] 74. 安全性加固
  - 实现 CSRF 保护
  - 实现 SQL 注入防护
  - 实现密码加密存储
  - 实现访问控制和授权检查
  - _需求: 1.1, 9.1_
