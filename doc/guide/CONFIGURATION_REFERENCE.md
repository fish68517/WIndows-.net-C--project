# 旅游平台 - 配置参考指南

## 目录
1. [appsettings.json 配置](#appsettingsjson-配置)
2. [数据库连接字符串](#数据库连接字符串)
3. [环境特定配置](#环境特定配置)
4. [应用设置](#应用设置)
5. [外部 API 配置](#外部-api-配置)
6. [文件上传配置](#文件上传配置)
7. [缓存配置](#缓存配置)
8. [日志配置](#日志配置)
9. [Kestrel 服务器配置](#kestrel-服务器配置)

---

## appsettings.json 配置

### 完整配置示例

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourismPlatformDb;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "Console": {
      "IncludeScopes": true
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "AdminUsername": "admin",
    "AdminPassword": "Admin@123456",
    "JwtSecret": "your-secret-key-change-in-production",
    "JwtExpireMinutes": 1440
  },
  "ExternalApis": {
    "WeatherApi": {
      "BaseUrl": "https://api.weatherapi.com/v1",
      "ApiKey": "your-weather-api-key"
    }
  },
  "FileUpload": {
    "MaxFileSizeBytes": 5242880,
    "AllowedExtensions": ".jpg,.jpeg,.png,.gif",
    "UploadPath": "uploads"
  },
  "Cache": {
    "DefaultDurationMinutes": 60,
    "WeatherCacheDurationMinutes": 180,
    "AttractionListCacheDurationMinutes": 60,
    "HotSpotsCacheDurationMinutes": 360
  },
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
}
```

---

## 数据库连接字符串

### LocalDB (Windows - 推荐用于开发)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourismPlatformDb;Trusted_Connection=true;"
}
```

**参数说明:**
- `Server`: LocalDB 实例名称
- `Database`: 数据库名称
- `Trusted_Connection`: 使用 Windows 认证

### SQL Server Express (Windows)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=TourismPlatformDb;Trusted_Connection=true;"
}
```

**参数说明:**
- `Server`: SQL Server 实例名称 (`.` 表示本地)
- `Database`: 数据库名称
- `Trusted_Connection`: 使用 Windows 认证

### SQL Server (具体实例)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TourismPlatformDb;User Id=sa;Password=YOUR_PASSWORD;"
}
```

**参数说明:**
- `Server`: SQL Server 服务器地址
- `Database`: 数据库名称
- `User Id`: 用户名
- `Password`: 密码

### SQL Server (远程服务器)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=192.168.1.100,1433;Database=TourismPlatformDb;User Id=sa;Password=YOUR_PASSWORD;"
}
```

**参数说明:**
- `Server`: 远程服务器 IP 和端口
- `Database`: 数据库名称
- `User Id`: 用户名
- `Password`: 密码

### Azure SQL Database

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=TourismPlatformDb;Persist Security Info=False;User ID=your-username;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
```

### Docker SQL Server

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=TourismPlatformDb;User Id=sa;Password=YourPassword123!;"
}
```

---

## 环境特定配置

### appsettings.Development.json

用于开发环境:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss"
    }
  },
  "AppSettings": {
    "AdminUsername": "admin",
    "AdminPassword": "Admin@123456"
  }
}
```

### appsettings.Production.json

用于生产环境:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_PRODUCTION_SERVER;Database=TourismPlatformDb;User Id=sa;Password=YOUR_SECURE_PASSWORD;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com,www.yourdomain.com",
  "AppSettings": {
    "AdminUsername": "admin",
    "AdminPassword": "YOUR_SECURE_PASSWORD",
    "JwtSecret": "your-very-secure-secret-key-change-this",
    "JwtExpireMinutes": 1440
  },
  "ExternalApis": {
    "WeatherApi": {
      "BaseUrl": "https://api.weatherapi.com/v1",
      "ApiKey": "your-production-weather-api-key"
    }
  },
  "FileUpload": {
    "MaxFileSizeBytes": 5242880,
    "AllowedExtensions": ".jpg,.jpeg,.png,.gif",
    "UploadPath": "/var/www/uploads"
  },
  "Cache": {
    "DefaultDurationMinutes": 120,
    "WeatherCacheDurationMinutes": 360,
    "AttractionListCacheDurationMinutes": 120,
    "HotSpotsCacheDurationMinutes": 720
  }
}
```

---

## 应用设置

### AppSettings 部分

```json
"AppSettings": {
  "AdminUsername": "admin",
  "AdminPassword": "Admin@123456",
  "JwtSecret": "your-secret-key-change-in-production",
  "JwtExpireMinutes": 1440
}
```

**参数说明:**

| 参数 | 说明 | 默认值 | 示例 |
|------|------|--------|------|
| AdminUsername | 默认管理员用户名 | admin | admin |
| AdminPassword | 默认管理员密码 | Admin@123456 | MySecurePassword123! |
| JwtSecret | JWT 令牌密钥 | your-secret-key-change-in-production | your-very-secure-secret-key |
| JwtExpireMinutes | JWT 令牌过期时间(分钟) | 1440 | 1440 (24小时) |

**安全建议:**
- 生产环境中修改 AdminPassword
- 生产环境中使用强 JwtSecret
- 定期更新密钥

---

## 外部 API 配置

### WeatherAPI 配置

```json
"ExternalApis": {
  "WeatherApi": {
    "BaseUrl": "https://api.weatherapi.com/v1",
    "ApiKey": "your-weather-api-key"
  }
}
```

**获取 API Key:**

1. 访问 [WeatherAPI.com](https://www.weatherapi.com/)
2. 点击 "Sign Up" 注册账户
3. 验证邮箱
4. 登录后在 Dashboard 中复制 API Key
5. 粘贴到配置文件中

**API 端点:**
- 当前天气: `/current.json`
- 预报: `/forecast.json`
- 历史数据: `/history.json`

**示例请求:**
```
https://api.weatherapi.com/v1/current.json?key=YOUR_API_KEY&q=Jinan&aqi=no
```

---

## 文件上传配置

### FileUpload 部分

```json
"FileUpload": {
  "MaxFileSizeBytes": 5242880,
  "AllowedExtensions": ".jpg,.jpeg,.png,.gif",
  "UploadPath": "uploads"
}
```

**参数说明:**

| 参数 | 说明 | 默认值 | 示例 |
|------|------|--------|------|
| MaxFileSizeBytes | 最大文件大小(字节) | 5242880 (5MB) | 10485760 (10MB) |
| AllowedExtensions | 允许的文件扩展名 | .jpg,.jpeg,.png,.gif | .jpg,.jpeg,.png,.gif,.webp |
| UploadPath | 上传文件保存路径 | uploads | /var/www/uploads |

**文件大小参考:**
- 1MB = 1048576 字节
- 5MB = 5242880 字节
- 10MB = 10485760 字节
- 20MB = 20971520 字节

**上传目录结构:**
```
wwwroot/
├── uploads/
│   ├── avatars/      # 用户头像
│   ├── attractions/  # 景点图片
│   └── diaries/      # 游记图片
```

---

## 缓存配置

### Cache 部分

```json
"Cache": {
  "DefaultDurationMinutes": 60,
  "WeatherCacheDurationMinutes": 180,
  "AttractionListCacheDurationMinutes": 60,
  "HotSpotsCacheDurationMinutes": 360
}
```

**参数说明:**

| 参数 | 说明 | 默认值 | 建议值 |
|------|------|--------|--------|
| DefaultDurationMinutes | 默认缓存时间 | 60 | 60-120 |
| WeatherCacheDurationMinutes | 天气数据缓存时间 | 180 | 180-360 |
| AttractionListCacheDurationMinutes | 景点列表缓存时间 | 60 | 60-120 |
| HotSpotsCacheDurationMinutes | 热门景点缓存时间 | 360 | 360-720 |

**缓存策略:**
- 频繁变化的数据: 60-120 分钟
- 相对稳定的数据: 180-360 分钟
- 很少变化的数据: 360-720 分钟

---

## 日志配置

### Logging 部分

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore": "Warning"
  },
  "Console": {
    "IncludeScopes": true
  }
}
```

**日志级别:**

| 级别 | 说明 | 用途 |
|------|------|------|
| Trace | 最详细的日志 | 调试 |
| Debug | 调试信息 | 开发 |
| Information | 一般信息 | 生产 |
| Warning | 警告信息 | 生产 |
| Error | 错误信息 | 生产 |
| Critical | 严重错误 | 生产 |
| None | 不记录 | 禁用 |

**开发环境配置:**
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Information",
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

**生产环境配置:**
```json
"Logging": {
  "LogLevel": {
    "Default": "Warning",
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore": "Warning"
  }
}
```

---

## Kestrel 服务器配置

### 基本配置

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

### 自定义端口

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:8080"
    },
    "Https": {
      "Url": "https://localhost:8443"
    }
  }
}
```

### 绑定到所有网卡

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://0.0.0.0:5000"
    },
    "Https": {
      "Url": "https://0.0.0.0:5001"
    }
  }
}
```

### SSL 证书配置

```json
"Kestrel": {
  "Endpoints": {
    "Https": {
      "Url": "https://localhost:5001",
      "Certificate": {
        "Path": "/path/to/certificate.pfx",
        "Password": "certificate-password"
      }
    }
  }
}
```

---

## 环境变量配置

### 使用环境变量覆盖配置

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;"
$env:AppSettings__AdminPassword = "NewPassword123!"

# Windows CMD
set ConnectionStrings__DefaultConnection=Server=...;Database=...;
set AppSettings__AdminPassword=NewPassword123!

# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=...;Database=...;"
export AppSettings__AdminPassword="NewPassword123!"
```

### Docker 环境变量

```bash
docker run -e "ConnectionStrings__DefaultConnection=Server=...;Database=...;" \
           -e "AppSettings__AdminPassword=NewPassword123!" \
           tourism-platform:latest
```

---

## 配置优先级

ASP.NET Core 按以下顺序加载配置 (后面的覆盖前面的):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. 用户密钥 (仅开发环境)
4. 环境变量
5. 命令行参数

---

## 常见配置场景

### 场景 1: 本地开发

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourismPlatformDb;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### 场景 2: 团队开发 (共享 SQL Server)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=dev-server.company.com;Database=TourismPlatformDb;User Id=dev_user;Password=dev_password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 场景 3: 测试环境

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=test-server.company.com;Database=TourismPlatformDb_Test;User Id=test_user;Password=test_password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 场景 4: 生产环境

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server.company.com;Database=TourismPlatformDb;User Id=prod_user;Password=SECURE_PASSWORD;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "tourism.company.com,www.tourism.company.com"
}
```

---

## 配置验证

### 验证连接字符串

```bash
# 使用 dotnet CLI
dotnet ef dbcontext info

# 应该输出:
# Build started...
# Build succeeded.
# Provider name: Microsoft.EntityFrameworkCore.SqlServer
# Options: None
```

### 验证应用配置

```bash
# 在应用启动时检查日志
dotnet run

# 应该看到:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
```

---

## 安全建议

1. **不要在版本控制中提交敏感信息**
   - 使用 `.gitignore` 排除 `appsettings.Production.json`
   - 使用用户密钥存储敏感数据

2. **使用强密码**
   - 管理员密码: 至少 12 个字符，包含大小写字母、数字和特殊字符
   - JWT Secret: 至少 32 个字符

3. **定期更新密钥**
   - 每季度更新一次 JWT Secret
   - 定期更改管理员密码

4. **使用 HTTPS**
   - 生产环境必须使用 HTTPS
   - 使用有效的 SSL 证书

5. **限制文件上传**
   - 设置合理的文件大小限制
   - 只允许必要的文件类型

---

## 故障排除

### 配置不生效

**解决方案:**
1. 检查配置文件名称是否正确
2. 检查 JSON 格式是否有效
3. 重启应用
4. 检查环境变量是否覆盖了配置

### 连接字符串错误

**解决方案:**
1. 验证服务器地址和端口
2. 检查用户名和密码
3. 验证数据库是否存在
4. 检查防火墙设置

### 日志级别不生效

**解决方案:**
1. 检查 `appsettings.{Environment}.json` 是否覆盖了设置
2. 检查环境变量是否覆盖了设置
3. 重启应用

---

## 下一步

- 查看 [运行指南](./RUN_GUIDE.md)
- 查看 [快速开始指南](./QUICK_START.md)
- 查看 [安装指南](./INSTALLATION_GUIDE.md)

---

**配置完成！现在可以运行应用了。** 🚀

