下面是我把附件需求“抽象成可直接喂给 Kiro 写代码的开发规格（按 ASP.NET Core MVC 的 Controllers / Models / Views 结构）”。内容来源于你文档里的技术约束、技术栈与功能清单。

---

## 0. 项目硬性约束（Kiro 生成代码时必须遵守）

* **项目结构必须包含** ：`Controllers/`、`Models/`、`Views/` 三个目录。
* **数据库** ：SQL Server + EF Core；你只会在 `Startup.cs` 里用**连接字符串**连接数据库；使用 `MyDbContext`。
* **静态资源** ：`wwwroot/` 放网页图片/前端源码资源。
* **技术栈（用于代码生成的依赖边界）** ：ASP.NET Core MVC、C#、HTML/CSS/JS、Bootstrap5、ASPChart、SQL Server、EF Core。

---

## 1. 系统角色与范围

### 角色

* **游客/前台用户（注册登录后可下单、写游记、评论等）**
* **管理员（后台预设账号登录）**

### 核心业务对象（建议 Kiro 建模的实体集合）

> 下面这些实体基本能覆盖你文档所有功能点，Kiro 可按此生成 Models + DbSet + 迁移。

* 用户：`User`
* 管理员：`AdminUser`（或用同一张表加 Role）
* 景点：`Attraction`
* 景点图片：`AttractionImage`
* 行政区：`District`
* 景点类型：`AttractionCategory`
* 美食：`Food`（与景点/行政区关联）
* 酒店：`Hotel`
* 房型：`HotelRoomType`
* 酒店预订订单：`HotelOrder`
* 门票订单：`TicketOrder`
* 核销码：`VerifyCode`（关联订单，含状态）
* 收藏：`Favorite`（用户-景点/酒店）
* 评论：`Comment`（景点评论、游记评论可复用或拆表）
* 评分：`Rating`（建议与 Comment 合并，或单独）
* 旅行日记/游记：`TravelDiary`
* 官方线路：`TravelRoute`
* 公告/资讯：`Announcement`

---

## 2. 前台模块需求拆解（可直接变成 Controllers/Views 任务）

### 2.1 用户认证与安全

**必须功能**

* 注册、登录
* 修改个人资料：昵称、头像、简介（建议加联系方式字段）

**建议控制器**

* `AccountController`：Register/Login/Logout/ForgotPassword/ResetPassword
* `ProfileController`：Index/Edit/UploadAvatar

**建议视图**

* `Views/Account/Register.cshtml`、`Login.cshtml`、`ForgotPassword.cshtml`、`ResetPassword.cshtml`
* `Views/Profile/Index.cshtml`、`Edit.cshtml`

---

### 2.2 信息查询与展示

**浏览与筛选**

* 按行政区、景点类型筛选浏览
* 关键词搜索（按景点名称）

**景点详情页**

* 展示：名称、相册、开放时间、票价、地址、交通、联系方式
* 同页展示周边**美食**与 **合作酒店** （含参考价格、联系方式）

**建议控制器**

* `AttractionsController`：Index(筛选/搜索)、Detail
* `HotelsController`：ListByAttraction/Detail
* `FoodsController`：ListByAttraction/Detail

**建议视图**

* `Views/Attractions/Index.cshtml`、`Detail.cshtml`
* `Views/Hotels/Detail.cshtml`
* `Views/Foods/Detail.cshtml`

---

### 2.3 下单支付（模拟）

**门票模拟支付简单的模拟支付**

* 选择日期 → 创建订单 → 进入模拟支付 → 支付成功状态更新

**酒店预订模拟支付**

* 选择房型、入住/离店日期 → 创建订单 → 模拟支付

**订单核销**

* 支付后订单显示“待使用”
* 生成核销码
* 管理员可核销（核销后变“已使用/已入住”等）

**订单状态（建议 Kiro 固化为 enum）**

* TicketOrderStatus：`PendingPay / Paid / Cancelled / ToUse / Used`
* HotelOrderStatus：`PendingPay / Paid / Cancelled / ToUse / Used`（或加 CheckedIn）

**建议控制器**

* `OrdersController`
  * Ticket：Create/PayCallback/Detail/List
  * Hotel：Create/PayCallback/Detail/List
* `VerifyController`：GenerateCode / ShowCode（前台查看）

---

### 2.4 互动与社区

* 核销完成后：对景点评分并发表评论
* 发布图文游记；他人浏览与评论（类似论坛）
* 收藏景点/酒店

**建议控制器**

* `DiariesController`：Index/Detail/Create/Edit/Delete
* `CommentsController`：Create/Delete（后台也会用）
* `FavoritesController`：Add/Remove/List
* `RatingsController`：Create（或合并到评论）

---

### 2.5 实用工具

* 接入“济南市气象局 API”：未来 3–7 天天气
* 内置“AI 对话模拟”：自动回答常见问题（可先用固定 FAQ/规则库实现）

**建议控制器**

* `WeatherController`：Forecast
* `ChatController`：Index / Ask（返回答案）

---

### 2.6 个性化推荐（亮点）

* 首页或景点详情页“猜你喜欢”：依据**当前浏览内容 + 收藏记录**推荐同类型/相关热门景点

**推荐最小可落地规则（方便 Kiro 写）**

1. 同类别优先（Category 相同）
2. 同行政区加分（District 相同）
3. 热门加分（按点击量/收藏数/下单数排序）
4. 排除已收藏或已浏览过多次的项（可选）

**建议控制器**

* `RecommendController`：ForHome / ForAttraction(id)

---

## 3. 个人中心（前台）

集中管理：

* 基本信息管理（Profile）
* 我的内容（游记/评论管理）
* 我的收藏（景点/酒店）
* 我的订单（门票/酒店，按类型与状态筛选）

**建议控制器**

* `MeController`：Dashboard
* `MeContentController`：MyDiaries/MyComments
* `MeFavoritesController`：Index
* `MeOrdersController`：Tickets/Hotels/Detail/Filter

---

## 4. 后台管理模块（管理员）

### 4.1 管理员登录

* 仅允许预设管理员账号登录

### 4.2 全站内容管理（CRUD）

* 景点/美食/酒店 CRUD
* 官方旅游线路发布：包含行程安排、景点介绍、交通建议
* 审核/删除用户评论与游记（违规内容处理）

### 4.3 用户管理

* 用户列表查询（按用户名、注册时间筛选）
* 违规用户禁用

### 4.4 订单管理与导出

* 门票订单：按状态筛选、更新状态、退款审核
* 酒店订单：按状态筛选、处理订单事务
* 订单导出 Excel（按时间、景点/线路条件）

### 4.5 数据统计与可视化

* 总用户数、总景点数、总游记数等
* ASPChart：热门景点（按点击量）趋势图

### 4.6 公告/资讯管理

* 发布全站公告（维护通知、节假日提示、活动推介）

**建议后台控制器（放 Areas/Admin 更清晰）**

* `AdminAccountController`
* `AdminAttractionsController`、`AdminHotelsController`、`AdminFoodsController`
* `AdminRoutesController`
* `AdminUsersController`
* `AdminOrdersController`（Ticket/Hotel）
* `AdminVerifyController`（核销）
* `AdminStatsController`（ASPChart 图表输出）
* `AdminAnnouncementsController`

---

## 5. 给 Kiro 的“生成任务清单”（按代码产物拆分）

你可以把下面整段直接贴进 Kiro，当作开发 TODO（它会更容易分步产出代码）：

1. 初始化 ASP.NET Core MVC 项目：创建 `Controllers/Models/Views/wwwroot`；配置 EF Core + SQL Server 连接字符串写在 `Startup.cs`；实现 `MyDbContext` 并迁移建表。
2. 实现用户注册登录、个人资料修改
3. 实现景点列表页：行政区/类型筛选 + 关键词搜索；景点详情页：相册、票价、开放时间、地址交通联系方式；详情页下方展示周边美食与酒店。
4. 实现门票订单与酒店订单：创建订单→简单的模拟支付→支付回调更新状态→生成核销码→订单状态流转（待支付/已支付/待使用/已使用等）。
5. 实现评论评分：仅“核销完成/已使用”后可评分评论；实现游记发布（图文）与游记评论；实现收藏景点/酒店。
6. 实现实用工具：天气 API（3–7 天）与 AI 对话模拟（FAQ/规则库版）。
7. 实现推荐：根据当前浏览与收藏记录，推荐同类型/同区域 + 热门景点。
8. 实现个人中心：我的内容、我的收藏、我的订单（按类型/状态筛选）。
9. 实现后台：管理员预设账号登录；景点/美食/酒店 CRUD；线路发布；评论/游记审核删除；用户查询与禁用；订单管理、退款审核、Excel 导出；统计报表 + ASPChart 图表；公告管理。

---

为了简单不需要测试任务
