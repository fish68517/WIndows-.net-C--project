# 旅游平台 - 项目运行指南

## 目录

1. [系统要求](#系统要求)
2. [SDK 安装](#sdk-安装)
3. [环境配置](#环境配置)
4. [项目设置](#项目设置)
5. [数据库配置](#数据库配置)
6. [运行项目](#运行项目)
7. [访问应用](#访问应用)
8. [常见问题](#常见问题)
9. [故障排除](#故障排除)

---

## 系统要求

### 操作系统

- Windows 10 或更高版本
- macOS 10.15 或更高版本
- Linux (Ubuntu 18.04 或更高版本)

### 硬件要求

- CPU: 双核或更高
- 内存: 4GB 或更高
- 磁盘空间: 至少 2GB 可用空间

### 软件要求

- .NET 8.0 SDK
- SQL Server 2019 或更高版本 / SQL Server Express / LocalDB
- Git (可选，用于版本控制)

---

## SDK 安装

### 1. 安装 .NET 8.0 SDK

#### Windows 用户

**方法 A: 使用官方安装程序**

1. 访问 [.NET 官方网站](https://dotnet.microsoft.com/download/dotnet/8.0)
2. 下载 .NET 8.0 SDK (Windows x64)
3. 运行安装程序，按照提示完成安装
4. 重启计算机

**方法 B: 使用 Chocolatey (如果已安装)**

```powershell
choco install dotnet-sdk-8.0
```

**方法 C: 使用 Windows Package Manager**

```powershell
winget install Microsoft.DotNet.SDK.8
```

#### macOS 用户

**方法 A: 使用官方安装程序**

1. 访问 [.NET 官方网站](https://dotnet.microsoft.com/download/dotnet/8.0)
2. 下载 .NET 8.0 SDK (macOS)
3. 运行安装程序

**方法 B: 使用 Homebrew**

```bash
brew install dotnet-sdk@8
```

#### Linux 用户 (Ubuntu)

```bash
# 添加 Microsoft 包存储库
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 安装 .NET 8.0 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### 2. 验证 SDK 安装

打开命令行/终端，运行以下命令验证安装：

```bash
dotnet --version
```

应该输出类似以下内容：

```
8.0.0
```

### 3. 安装 Entity Framework Core 工具

```bash
dotnet tool install --global dotnet-ef
```

验证安装：

```bash
dotnet ef --version
```

---

## 环境配置

### 1. 安装 SQL Server 或 LocalDB

#### Windows 用户 - 使用 LocalDB (推荐用于开发)

LocalDB 通常随 Visual Studio 一起安装。如果未安装，可以：

1. 访问 [SQL Server Express 下载页面](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
2. 下载 SQL Server Express
3. 在安装过程中选择 "LocalDB" 选项

验证 LocalDB 安装：

```bash
sqllocaldb info
```

#### Windows 用户 - 使用 SQL Server Express

1. 访问 [SQL Server Express 下载页面](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
2. 下载并安装 SQL Server Express
3. 在安装过程中记住实例名称和 SA 密码

#### macOS/Linux 用户 - 使用 Docker

```bash
# 拉取 SQL Server 镜像
docker pull mcr.microsoft.com/mssql/server:2022-latest

# 运行 SQL Server 容器
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. 配置连接字符串

编辑 `appsettings.json` 文件中的连接字符串：

**对于 LocalDB (Windows):**

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourismPlatformDb;Trusted_Connection=true;"
}
```

**对于 SQL Server Express:**

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=TourismPlatformDb;Trusted_Connection=true;"
}
```

**对于 SQL Server (远程或指定实例):**

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TourismPlatformDb;User Id=sa;Password=YOUR_PASSWORD;"
}
```

**对于 Docker SQL Server:**

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=TourismPlatformDb;User Id=sa;Password=YourPassword123!;"
}
```

### 3. 配置外部 API (可选)

如果需要使用天气功能，编辑 `appsettings.json` 中的天气 API 配置：

```json
"ExternalApis": {
  "WeatherApi": {
    "BaseUrl": "https://api.weatherapi.com/v1",
    "ApiKey": "your-weather-api-key"
  }
}
```

获取 API Key:

1. 访问 [WeatherAPI.com](https://www.weatherapi.com/)
2. 注册免费账户
3. 复制 API Key 到配置文件

---

## 项目设置

### 1. 克隆或下载项目

如果使用 Git:

```bash
git clone <repository-url>
cd TourismPlatform
```

或直接下载项目文件并解压。

### 2. 恢复 NuGet 包

```bash
dotnet restore
```

这将下载项目所需的所有 NuGet 包。

### 3. 验证项目结构

确保项目包含以下主要目录：

```
TourismPlatform/
├── Controllers/              # 前台控制器
├── Areas/Admin/Controllers/  # 后台管理控制器
├── Models/                   # 数据模型
├── Services/                 # 业务逻辑层
├── Repositories/             # 数据访问层
├── Views/                    # 前台视图
├── Areas/Admin/Views/        # 后台视图
├── Data/                     # 数据库上下文
├── wwwroot/                  # 静态资源
├── appsettings.json          # 应用配置
└── TourismPlatform.csproj    # 项目文件
```

---

## 数据库配置

### 1. 创建数据库迁移

```bash
dotnet ef migrations add InitialCreate
```

这将在 `Data/Migrations` 目录中创建迁移文件。

### 2. 应用迁移到数据库

```bash
dotnet ef database update
```

这将：

- 创建 `TourismPlatformDb` 数据库
- 创建所有数据表
- 设置所有关系和约束
- 初始化种子数据（默认管理员账户和基础数据）

### 3. 验证数据库创建

**Windows 用户 (使用 SQL Server Management Studio):**

1. 打开 SQL Server Management Studio
2. 连接到 `(localdb)\mssqllocaldb` 或你的 SQL Server 实例
3. 在对象浏览器中查看 `TourismPlatformDb` 数据库
4. 展开数据库查看所有表

**所有用户 (使用命令行):**

```bash
# 查看数据库中的表
dotnet ef dbcontext info
```

### 4. 种子数据

数据库初始化时会自动创建以下种子数据：

**默认管理员账户:**

- 用户名: `admin`
- 密码: `admin123`

**行政区数据:**

- 市中区
- 历下区
- 槐荫区
- 天桥区
- 历城区
- 长清区

**景点类型:**

- 历史遗迹
- 自然风景
- 文化场所
- 主题公园
- 宗教场所

---

## 运行项目

### 1. 使用 dotnet CLI 运行

```bash
dotnet run
```

应用将在以下地址启动：

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### 2. 使用 Visual Studio 运行

1. 打开 `TourismPlatform.sln` 文件
2. 按 `F5` 或点击 "运行" 按钮
3. 应用将在默认浏览器中打开

### 3. 使用 Visual Studio Code 运行

1. 打开项目文件夹
2. 按 `Ctrl+F5` 或使用命令面板 (`Ctrl+Shift+P`) 运行 "Run Without Debugging"
3. 应用将在终端中启动

### 4. 使用 Docker 运行 (可选)

**创建 Dockerfile:**

在项目根目录创建 `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TourismPlatform.csproj", "./"]
RUN dotnet restore "TourismPlatform.csproj"
COPY . .
RUN dotnet build "TourismPlatform.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "TourismPlatform.dll"]
```

**构建和运行:**

```bash
# 构建镜像
docker build -t tourism-platform .

# 运行容器
docker run -p 5000:80 -p 5001:443 tourism-platform
```

---

## 访问应用

### 1. 前台应用

打开浏览器访问：

```
https://localhost:5001
```

**主要功能:**

- 首页: 查看最新公告和推荐景点
- 景点浏览: `/Attractions` - 浏览所有景点
- 游记: `/Diaries` - 查看和发布游记
- 天气: `/Weather/Forecast` - 查看天气预报
- AI助手: `/Chat` - 与 AI 对话
- 个人中心: `/Me/Dashboard` - 管理个人信息和订单

### 2. 后台管理系统

访问管理员登录页面：

```
https://localhost:5001/Admin/AdminAccount/Login
```

**默认管理员账户:**

- 用户名: `admin`
- 密码: `admin123`

**管理功能:**

- 景点管理: 添加、编辑、删除景点
- 酒店管理: 管理酒店和房间类型
- 美食管理: 管理美食信息
- 线路管理: 创建旅游线路
- 用户管理: 管理用户账户
- 订单管理: 查看和管理订单
- 核销管理: 验证和核销订单
- 评论审核: 审核用户评论
- 游记审核: 审核用户游记
- 数据统计: 查看平台数据统计
- 公告管理: 发布和管理公告

### 3. 测试用户注册

1. 访问前台首页
2. 点击 "注册" 按钮
3. 填写注册表单
4. 完成注册后可以登录

---

## 常见问题

### Q1: 如何更改管理员密码？

编辑 `appsettings.json` 中的 `AdminPassword` 字段，然后重新运行数据库迁移。

### Q2: 如何修改数据库连接字符串？

编辑 `appsettings.json` 中的 `ConnectionStrings.DefaultConnection` 字段。

### Q3: 如何添加新的景点类型？

1. 登录后台管理系统
2. 在数据库中直接插入数据到 `AttractionCategories` 表
3. 或在 `SeedData.cs` 中添加新的类型

### Q4: 如何配置文件上传路径？

编辑 `appsettings.json` 中的 `FileUpload.UploadPath` 字段。

### Q5: 如何启用 HTTPS？

HTTPS 在开发环境中默认启用。生产环境需要配置 SSL 证书。

### Q6: 如何修改应用端口？

在 `appsettings.json` 中添加以下配置：

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:5000"
    },
    "Https": {
      "Url": "https://localhost:5001"
    }
  }
}
```

### Q7: 如何查看应用日志？

日志输出到控制台。可以在 `appsettings.json` 中修改日志级别：

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Information"
  }
}
```

---

## 故障排除

### 问题 1: "无法连接到数据库"

**解决方案:**

1. 验证 SQL Server 或 LocalDB 是否正在运行
2. 检查 `appsettings.json` 中的连接字符串
3. 确保数据库已创建：`dotnet ef database update`
4. 检查防火墙设置

### 问题 2: "迁移失败"

**解决方案:**

1. 删除 `Data/Migrations` 目录中的所有迁移文件（除了 `__EFMigrationsHistory` 表）
2. 重新创建迁移：`dotnet ef migrations add InitialCreate`
3. 应用迁移：`dotnet ef database update`

### 问题 3: "NuGet 包恢复失败"

**解决方案:**

1. 清除 NuGet 缓存：`dotnet nuget locals all --clear`
2. 重新恢复包：`dotnet restore`
3. 检查网络连接

### 问题 4: "端口已被占用"

**解决方案:**

1. 查找占用端口的进程：
   - Windows: `netstat -ano | findstr :5001`
   - macOS/Linux: `lsof -i :5001`
2. 终止进程或修改应用端口

### 问题 5: "登录失败"

**解决方案:**

1. 确保数据库已初始化并包含种子数据
2. 检查用户名和密码是否正确
3. 查看应用日志获取更多信息

### 问题 6: "文件上传失败"

**解决方案:**

1. 检查 `wwwroot/uploads` 目录是否存在
2. 确保应用有写入权限
3. 检查 `appsettings.json` 中的文件大小限制

### 问题 7: "天气功能不工作"

**解决方案:**

1. 检查是否配置了 WeatherAPI Key
2. 验证 API Key 是否有效
3. 检查网络连接
4. 查看应用日志获取错误信息

### 问题 8: "静态资源加载失败"

**解决方案:**

1. 确保 `wwwroot` 目录存在
2. 检查 CSS 和 JavaScript 文件路径
3. 清除浏览器缓存
4. 检查浏览器开发者工具中的网络标签

---

## 开发工作流

### 1. 创建新的数据库迁移

当修改数据模型时：

```bash
# 创建新迁移
dotnet ef migrations add DescriptionOfChanges

# 应用迁移
dotnet ef database update
```

### 2. 回滚迁移

```bash
# 回滚到上一个迁移
dotnet ef database update PreviousMigrationName

# 删除最后一个迁移
dotnet ef migrations remove
```

### 3. 查看迁移历史

```bash
dotnet ef migrations list
```

### 4. 生成数据库脚本

```bash
# 生成 SQL 脚本
dotnet ef migrations script
```

---

## 性能优化建议

### 1. 启用缓存

在 `appsettings.json` 中配置缓存时间：

```json
"Cache": {
  "DefaultDurationMinutes": 60,
  "WeatherCacheDurationMinutes": 180,
  "AttractionListCacheDurationMinutes": 60
}
```

### 2. 数据库索引

确保在频繁查询的字段上创建索引。已配置的索引：

- Email (Users 表)
- Code (VerifyCodes 表)
- UserId + AttractionId (Favorites 表)

### 3. 异步操作

所有数据库操作都使用异步方法以提高性能。

### 4. 连接池

Entity Framework Core 自动管理连接池。

---

## 部署到生产环境

### 1. 发布应用

```bash
dotnet publish -c Release -o ./publish
```

### 2. 配置生产环境

创建 `appsettings.Production.json`:

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

### 3. 配置 HTTPS

- 获取 SSL 证书
- 在 IIS 或 Nginx 中配置 HTTPS
- 更新应用配置

### 4. 数据库备份

定期备份生产数据库：

```bash
# SQL Server 备份
BACKUP DATABASE TourismPlatformDb TO DISK = 'C:\Backups\TourismPlatformDb.bak'
```

---

## 获取帮助

### 文档和资源

- [.NET 官方文档](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core 文档](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core 文档](https://docs.microsoft.com/ef/core/)
- [项目需求文档](./requirements.md)
- [项目设计文档](./design.md)

### 联系支持

如遇到问题，请：

1. 查看本指南的故障排除部分
2. 检查应用日志
3. 查看项目文档
4. 联系开发团队

---

## 版本信息

- **项目名称**: 旅游平台
- **.NET 版本**: 8.0
- **数据库**: SQL Server 2019+
- **最后更新**: 2024年1月

---

**祝你使用愉快！** 🎉
