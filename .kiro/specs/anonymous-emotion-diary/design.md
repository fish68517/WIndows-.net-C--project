# 设计文档 - 匿名情绪日记应用

## 概述

匿名情绪日记应用采用 C# .NET 技术栈，使用 MVC（Model-View-Controller）架构模式。系统包含用户认证、日记管理、情绪检测和日志记录等核心功能。应用使用本地数据库（SQLite 或 SQL Server LocalDB）存储用户和日记数据。

## 架构设计

### MVC 架构层次

```
┌─────────────────────────────────────────────────────┐
│                    表现层 (View)                      │
│  - WinForms/WPF 用户界面                             │
│  - 登录界面、日记编辑界面、日记列表界面              │
└─────────────────────────────────────────────────────┘
                          ↕
┌─────────────────────────────────────────────────────┐
│                  控制层 (Controller)                  │
│  - 用户认证控制器                                    │
│  - 日记管理控制器                                    │
│  - 情绪分析控制器                                    │
└─────────────────────────────────────────────────────┘
                          ↕
┌─────────────────────────────────────────────────────┐
│                   业务层 (Model)                      │
│  - 用户服务、日记服务、情绪检测服务                  │
│  - 数据验证、业务逻辑处理                            │
└─────────────────────────────────────────────────────┘
                          ↕
┌─────────────────────────────────────────────────────┐
│                   数据访问层                          │
│  - 数据库连接管理                                    │
│  - 用户数据访问对象 (DAO)                            │
│  - 日记数据访问对象 (DAO)                            │
└─────────────────────────────────────────────────────┘
                          ↕
┌─────────────────────────────────────────────────────┐
│                  本地数据库 (SQLite)                  │
│  - 用户表、日记表、日志表                            │
└─────────────────────────────────────────────────────┘
```

## 组件与接口

### 1. 数据模型 (Model)

#### 用户模型 (User)
```
属性:
- UserId: int (主键)
- Username: string (用户名，唯一)
- PasswordHash: string (密码哈希值)
- CreatedAt: DateTime (创建时间)
- LastLoginAt: DateTime (最后登录时间)
```

#### 日记模型 (Diary)
```
属性:
- DiaryId: int (主键)
- UserId: int (外键，关联用户)
- Content: string (日记内容)
- EmotionIndex: int (情绪指数，0-100)
- CreatedAt: DateTime (创建时间)
- IsHighRisk: bool (是否为高风险)
```

#### 日志模型 (Log)
```
属性:
- LogId: int (主键)
- LogType: string (日志类型：Debug/Error/EmotionAnalysis)
- Message: string (日志信息)
- StackTrace: string (堆栈跟踪，仅错误日志)
- CreatedAt: DateTime (创建时间)
```

### 2. 业务服务层 (Service)

#### 用户服务 (UserService)
```
方法:
- RegisterUser(username, password): bool
- LoginUser(username, password): User
- ValidateUsername(username): bool
- ValidatePassword(password): bool
- GetUserById(userId): User
```

#### 日记服务 (DiaryService)
```
方法:
- CreateDiary(userId, content): Diary
- GetDiaryById(diaryId): Diary
- GetUserDiaries(userId): List<Diary>
- DeleteDiary(diaryId): bool
- ValidateDiaryContent(content): bool
```

#### 情绪检测服务 (EmotionDetectionService)
```
方法:
- AnalyzeEmotion(content): int (返回情绪指数)
- ExtractKeywords(content): List<string>
- CallLLMAPI(content): EmotionResult
- IsHighRisk(emotionIndex): bool
- GetRiskWarning(emotionIndex): string
```

#### 日志服务 (LogService)
```
方法:
- LogDebug(message): void
- LogError(message, exception): void
- LogEmotionAnalysis(diaryId, emotionIndex, analysisTime): void
- RotateLogFile(): void
```

### 3. 数据访问层 (DAO/Repository)

#### 用户数据访问对象 (UserDAO)
```
方法:
- InsertUser(user): bool
- SelectUserByUsername(username): User
- SelectUserById(userId): User
- UpdateLastLogin(userId): bool
```

#### 日记数据访问对象 (DiaryDAO)
```
方法:
- InsertDiary(diary): bool
- SelectDiaryById(diaryId): Diary
- SelectDiariesByUserId(userId): List<Diary>
- DeleteDiary(diaryId): bool
- UpdateDiary(diary): bool
```

#### 日志数据访问对象 (LogDAO)
```
方法:
- InsertLog(log): bool
- SelectLogsByType(logType): List<Log>
- DeleteOldLogs(days): int
```

### 4. 控制层 (Controller)

#### 用户认证控制器 (AuthController)
```
方法:
- HandleRegister(username, password): void
- HandleLogin(username, password): void
- HandleLogout(): void
```

#### 日记管理控制器 (DiaryController)
```
方法:
- HandleCreateDiary(content): void
- HandleViewDiaries(): void
- HandleDeleteDiary(diaryId): void
- HandleViewDiaryDetail(diaryId): void
```

#### 情绪分析控制器 (EmotionController)
```
方法:
- HandleEmotionAnalysis(content): void
- HandleDisplayWarning(emotionIndex): void
```

### 5. 表现层 (View)

#### 登录界面 (LoginView)
- 用户名输入框
- 密码输入框
- 登录按钮
- 注册按钮
- 错误提示标签

#### 注册界面 (RegisterView)
- 用户名输入框
- 密码输入框
- 确认密码输入框
- 注册按钮
- 返回登录按钮
- 验证错误提示

#### 日记编辑界面 (DiaryEditView)
- 日记内容文本框
- 发布按钮
- 返回按钮
- 字数统计标签

#### 日记列表界面 (DiaryListView)
- 日记列表（显示摘要、时间、情绪指数）
- 删除按钮
- 查看详情按钮
- 新建日记按钮

#### 日记详情界面 (DiaryDetailView)
- 日记完整内容
- 发布时间
- 情绪指数显示
- 高风险标记（如适用）
- 返回按钮

## 数据模型

### 数据库表结构

#### Users 表
```sql
CREATE TABLE Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME
);
```

#### Diaries 表
```sql
CREATE TABLE Diaries (
    DiaryId INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    Content TEXT NOT NULL,
    EmotionIndex INTEGER NOT NULL,
    IsHighRisk BOOLEAN DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
```

#### Logs 表
```sql
CREATE TABLE Logs (
    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
    LogType TEXT NOT NULL,
    Message TEXT NOT NULL,
    StackTrace TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### 数据关系
- Users 与 Diaries：一对多关系（一个用户可以有多篇日记）
- Logs 表独立存储所有日志记录

## 错误处理

### 异常类型定义
```
- ValidationException: 数据验证失败
- DatabaseException: 数据库操作失败
- AuthenticationException: 用户认证失败
- EmotionAnalysisException: 情绪分析失败
- LLMAPIException: 大模型 API 调用失败
```

### 错误处理策略
1. **输入验证**: 在控制层和业务层进行双重验证
2. **数据库异常**: 捕获并记录到错误日志，向用户显示友好提示
3. **API 异常**: 当大模型 API 调用失败时，使用关键词识别作为备选方案
4. **日志异常**: 确保日志记录失败不会中断主业务流程

## 测试策略

### 单元测试
- 用户服务测试：注册、登录、验证逻辑
- 日记服务测试：创建、删除、查询日记
- 情绪检测服务测试：情绪指数计算、高风险判断
- 数据访问层测试：数据库 CRUD 操作

### 集成测试
- 用户注册到登录的完整流程
- 日记创建到情绪分析的完整流程
- 日记删除和数据库一致性验证

### 界面测试
- 登录界面的输入验证和错误提示
- 日记编辑界面的字数限制和发布流程
- 日记列表界面的排序和删除功能
- 高风险预警提示的显示

## 技术选型

### 开发框架
- **语言**: C# .NET Framework 4.7+ 或 .NET 6+
- **UI 框架**: WinForms 或 WPF
- **数据库**: SQLite（轻量级，无需额外安装）或 SQL Server LocalDB

### 外部依赖
- **密码加密**: BCrypt.Net-Next（密码哈希）
- **大模型 API**: 开源模型接口（如 Ollama、Hugging Face API）
- **日志框架**: NLog 或 Serilog（可选，简单应用可使用文件 I/O）

### 开发工具
- **IDE**: Visual Studio 2022 Community Edition
- **版本控制**: Git
- **数据库工具**: SQLite Studio 或 SQL Server Management Studio

## 部署与配置

### 配置文件 (app.config 或 appsettings.json)
```
- 数据库连接字符串
- 大模型 API 端点和密钥
- 情绪指数高风险阈值（默认 70）
- 日志文件路径和最大大小
- 日记内容最大长度（默认 5000 字符）
```

### 初始化流程
1. 检查本地数据库是否存在，不存在则创建
2. 执行数据库迁移脚本，创建必要的表
3. 初始化日志系统
4. 验证大模型 API 连接

## 安全考虑

1. **密码安全**: 使用 BCrypt 进行密码哈希存储，不存储明文密码
2. **匿名性**: 不收集用户真实身份信息，仅使用用户名
3. **数据隐私**: 日记数据仅存储在本地数据库，不上传到云端
4. **API 安全**: 大模型 API 调用使用 HTTPS，敏感信息不记录到日志
5. **输入验证**: 对所有用户输入进行严格验证，防止 SQL 注入和 XSS 攻击
