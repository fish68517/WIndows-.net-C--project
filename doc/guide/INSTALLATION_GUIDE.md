# 旅游平台 - 详细安装指南

## 目录
1. [Windows 安装指南](#windows-安装指南)
2. [macOS 安装指南](#macos-安装指南)
3. [Linux 安装指南](#linux-安装指南)
4. [Docker 安装指南](#docker-安装指南)
5. [验证安装](#验证安装)

---

## Windows 安装指南

### 步骤 1: 安装 .NET 8.0 SDK

#### 方法 A: 使用官方安装程序 (推荐)

1. 访问 [.NET 官方下载页面](https://dotnet.microsoft.com/download/dotnet/8.0)
2. 点击 "Download .NET SDK" (Windows x64)
3. 下载完成后，双击安装程序
4. 按照安装向导完成安装
5. 重启计算机

#### 方法 B: 使用 Chocolatey

前置条件: 已安装 Chocolatey

```powershell
# 以管理员身份打开 PowerShell
choco install dotnet-sdk-8.0
```

#### 方法 C: 使用 Windows Package Manager

前置条件: 已安装 Windows Package Manager

```powershell
winget install Microsoft.DotNet.SDK.8
```

### 步骤 2: 安装 SQL Server 或 LocalDB

#### 选项 A: 使用 LocalDB (推荐用于开发)

LocalDB 通常随 Visual Studio 一起安装。如果未安装：

1. 访问 [SQL Server Express 下载页面](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
2. 下载 SQL Server Express
3. 运行安装程序
4. 在 "Feature Selection" 中选择 "LocalDB"
5. 完成安装

验证 LocalDB:
```powershell
sqllocaldb info
```

#### 选项 B: 使用 SQL Server Express

1. 访问 [SQL Server Express 下载页面](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
2. 下载 SQL Server Express
3. 运行安装程序
4. 选择 "New SQL Server stand-alone installation"
5. 记住实例名称和 SA 密码
6. 完成安装

验证 SQL Server:
```powershell
# 打开 SQL Server Management Studio
# 连接到 .\SQLEXPRESS 或你的实例名称
```

### 步骤 3: 安装 Entity Framework Core 工具

```powershell
dotnet tool install --global dotnet-ef
```

验证安装:
```powershell
dotnet ef --version
```

### 步骤 4: 安装 Visual Studio (可选)

如果需要 IDE:

1. 访问 [Visual Studio 下载页面](https://visualstudio.microsoft.com/downloads/)
2. 下载 Visual Studio Community (免费)
3. 运行安装程序
4. 选择 "ASP.NET and web development" 工作负载
5. 完成安装

### 步骤 5: 克隆项目

```powershell
# 使用 Git
git clone <repository-url>
cd TourismPlatform

# 或直接下载并解压项目文件
```

### 步骤 6: 恢复 NuGet 包

```powershell
dotnet restore
```

### 步骤 7: 配置数据库连接

编辑 `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourismPlatformDb;Trusted_Connection=true;"
  }
}
```

### 步骤 8: 创建并应用数据库迁移

```powershell
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

### 步骤 9: 运行应用

```powershell
dotnet run
```

应用将在 `https://localhost:5001` 启动。

---

## macOS 安装指南

### 步骤 1: 安装 .NET 8.0 SDK

#### 方法 A: 使用官方安装程序

1. 访问 [.NET 官方下载页面](https://dotnet.microsoft.com/download/dotnet/8.0)
2. 下载 .NET 8.0 SDK (macOS)
3. 打开下载的 `.pkg` 文件
4. 按照安装向导完成安装
5. 重启终端

#### 方法 B: 使用 Homebrew (推荐)

```bash
# 安装 Homebrew (如果未安装)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# 安装 .NET 8.0 SDK
brew install dotnet-sdk@8
```

验证安装:
```bash
dotnet --version
```

### 步骤 2: 安装 SQL Server

#### 选项 A: 使用 Docker (推荐)

```bash
# 安装 Docker Desktop for Mac
# 访问 https://www.docker.com/products/docker-desktop

# 拉取 SQL Server 镜像
docker pull mcr.microsoft.com/mssql/server:2022-latest

# 运行 SQL Server 容器
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# 验证容器运行
docker ps
```

#### 选项 B: 使用 Homebrew

```bash
# 安装 SQL Server
brew install mssql-tools

# 启动 SQL Server (需要 Docker)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### 步骤 3: 安装 Entity Framework Core 工具

```bash
dotnet tool install --global dotnet-ef
```

验证安装:
```bash
dotnet ef --version
```

### 步骤 4: 安装 Visual Studio Code (可选)

```bash
# 使用 Homebrew
brew install visual-studio-code

# 或访问 https://code.visualstudio.com/
```

### 步骤 5: 克隆项目

```bash
# 使用 Git
git clone <repository-url>
cd TourismPlatform

# 或直接下载并解压项目文件
```

### 步骤 6: 恢复 NuGet 包

```bash
dotnet restore
```

### 步骤 7: 配置数据库连接

编辑 `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TourismPlatformDb;User Id=sa;Password=YourPassword123!;"
  }
}
```

### 步骤 8: 创建并应用数据库迁移

```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

### 步骤 9: 运行应用

```bash
dotnet run
```

应用将在 `https://localhost:5001` 启动。

---

## Linux 安装指南

### 步骤 1: 安装 .NET 8.0 SDK

#### Ubuntu/Debian

```bash
# 添加 Microsoft 包存储库
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 更新包列表
sudo apt-get update

# 安装 .NET 8.0 SDK
sudo apt-get install -y dotnet-sdk-8.0
```

#### Fedora/RHEL/CentOS

```bash
# 添加 Microsoft 包存储库
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo dnf install -y https://packages.microsoft.com/config/rhel/9/packages-microsoft-prod.rpm

# 安装 .NET 8.0 SDK
sudo dnf install -y dotnet-sdk-8.0
```

验证安装:
```bash
dotnet --version
```

### 步骤 2: 安装 SQL Server

#### 使用 Docker (推荐)

```bash
# 安装 Docker
sudo apt-get install -y docker.io

# 启动 Docker 服务
sudo systemctl start docker

# 拉取 SQL Server 镜像
sudo docker pull mcr.microsoft.com/mssql/server:2022-latest

# 运行 SQL Server 容器
sudo docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# 验证容器运行
sudo docker ps
```

#### 使用 SQL Server for Linux

```bash
# Ubuntu/Debian
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"
sudo apt-get update
sudo apt-get install -y mssql-server

# 运行安装脚本
sudo /opt/mssql/bin/mssql-conf setup
```

### 步骤 3: 安装 Entity Framework Core 工具

```bash
dotnet tool install --global dotnet-ef
```

验证安装:
```bash
dotnet ef --version
```

### 步骤 4: 安装 Visual Studio Code (可选)

```bash
# Ubuntu/Debian
sudo apt-get install -y code

# Fedora/RHEL
sudo dnf install -y code
```

### 步骤 5: 克隆项目

```bash
# 使用 Git
git clone <repository-url>
cd TourismPlatform

# 或直接下载并解压项目文件
```

### 步骤 6: 恢复 NuGet 包

```bash
dotnet restore
```

### 步骤 7: 配置数据库连接

编辑 `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TourismPlatformDb;User Id=sa;Password=YourPassword123!;"
  }
}
```

### 步骤 8: 创建并应用数据库迁移

```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

### 步骤 9: 运行应用

```bash
dotnet run
```

应用将在 `https://localhost:5001` 启动。

---

## Docker 安装指南

### 前置条件

- 已安装 Docker
- 已安装 Docker Compose (可选)

### 方法 1: 使用 Docker 运行应用

#### 步骤 1: 创建 Dockerfile

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

#### 步骤 2: 创建 .dockerignore

```
**/.classpath
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md
```

#### 步骤 3: 构建镜像

```bash
docker build -t tourism-platform:latest .
```

#### 步骤 4: 运行容器

```bash
docker run -p 5000:80 -p 5001:443 \
  -e "ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=TourismPlatformDb;User Id=sa;Password=YourPassword123!;" \
  tourism-platform:latest
```

### 方法 2: 使用 Docker Compose

#### 步骤 1: 创建 docker-compose.yml

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourPassword123!"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - tourism-network

  app:
    build: .
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=TourismPlatformDb;User Id=sa;Password=YourPassword123!;"
    depends_on:
      - sqlserver
    networks:
      - tourism-network

volumes:
  sqlserver_data:

networks:
  tourism-network:
    driver: bridge
```

#### 步骤 2: 启动服务

```bash
docker-compose up -d
```

#### 步骤 3: 查看日志

```bash
docker-compose logs -f app
```

#### 步骤 4: 停止服务

```bash
docker-compose down
```

---

## 验证安装

### 验证 .NET SDK

```bash
dotnet --version
```

应该输出类似:
```
8.0.0
```

### 验证 Entity Framework Core 工具

```bash
dotnet ef --version
```

应该输出类似:
```
Entity Framework Core .NET Command-line Tools 8.0.0
```

### 验证数据库连接

```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应该输出:
# Build started...
# Build succeeded.
# To undo this action, use 'ef migrations remove'
```

### 验证应用运行

```bash
dotnet run
```

应该输出类似:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 验证数据库初始化

```bash
dotnet ef database update
```

应该输出类似:
```
Build started...
Build succeeded.
Applying migration '20240120000000_InitialCreate'.
Done.
```

---

## 故障排除

### 问题: "dotnet: command not found"

**解决方案:**
- 重启终端或命令行
- 检查 .NET SDK 是否正确安装
- 检查 PATH 环境变量

### 问题: "无法连接到数据库"

**解决方案:**
- 验证 SQL Server 或 LocalDB 是否运行
- 检查连接字符串
- 检查防火墙设置

### 问题: "迁移失败"

**解决方案:**
- 删除 `Data/Migrations` 目录
- 重新创建迁移
- 检查数据模型是否有错误

### 问题: "NuGet 包恢复失败"

**解决方案:**
- 清除 NuGet 缓存: `dotnet nuget locals all --clear`
- 检查网络连接
- 重新运行 `dotnet restore`

---

## 下一步

- 查看 [快速开始指南](./QUICK_START.md)
- 查看 [完整运行指南](./RUN_GUIDE.md)
- 查看 [需求文档](./requirements.md)

---

**安装完成！现在可以开始开发了。** 🎉

