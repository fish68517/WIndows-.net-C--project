# 测试执行指南 - 匿名情绪日记应用

## 概述

本指南说明如何执行匿名情绪日记应用的端到端测试套件。测试验证了所有主要功能流程，包括用户认证、日记管理、情绪分析和日志记录。

## 前置条件

- .NET Framework 4.7+ 或 .NET 6+
- Visual Studio 2022 或其他 C# IDE
- SQLite 支持库（已通过 NuGet 包含）
- BCrypt.Net-Next NuGet 包（已通过 NuGet 包含）

## 测试文件位置

- **测试代码**: `AnonymousEmotionDiary/EndToEndTests.cs`
- **测试运行器**: `AnonymousEmotionDiary/TestRunner.cs`
- **测试报告**: `.kiro/specs/anonymous-emotion-diary/END_TO_END_TEST_REPORT.md`

## 执行方式

### 方式 1：通过 Visual Studio 运行

1. 打开 `AnonymousEmotionDiary.sln` 项目
2. 在解决方案资源管理器中找到 `TestRunner.cs`
3. 右键点击项目 → 属性 → 调试
4. 设置启动对象为 `AnonymousEmotionDiary.TestRunner`
5. 按 F5 或点击"开始调试"

### 方式 2：通过命令行运行

```bash
# 进入项目目录
cd AnonymousEmotionDiary

# 编译项目
dotnet build

# 运行测试
dotnet run -- TestRunner
```

### 方式 3：通过应用主程序运行

可以在 `Program.cs` 中添加以下代码在应用启动时运行测试：

```csharp
// 在 Program.cs 的 Main 方法中添加
if (args.Length > 0 && args[0] == "test")
{
    EndToEndTests tests = new EndToEndTests();
    tests.RunAllTests();
    return;
}
```

然后运行：
```bash
dotnet run -- test
```

## 测试执行流程

### 初始化阶段
1. 初始化日志系统
2. 初始化数据库
3. 创建必要的表结构

### 测试执行阶段
按顺序执行 7 个测试：

1. **用户注册流程** (Test 1)
   - 验证用户名和密码验证规则
   - 验证用户创建和数据库存储
   - 验证重复用户名拒绝

2. **用户登录流程** (Test 2)
   - 验证正确凭证接受
   - 验证错误凭证拒绝
   - 验证最后登录时间戳更新

3. **日记创建流程** (Test 3)
   - 验证日记内容验证
   - 验证情绪分析执行
   - 验证日记数据库存储

4. **高风险情绪检测** (Test 4)
   - 验证高风险阈值检测
   - 验证预警信息生成
   - 验证低风险日记正确处理

5. **日记查看流程** (Test 5)
   - 验证日记列表检索
   - 验证日记排序 (最新优先)
   - 验证日记详情显示

6. **日记删除流程** (Test 6)
   - 验证日记删除
   - 验证数据库更新
   - 验证列表刷新

7. **日志系统** (Test 7)
   - 验证调试日志记录
   - 验证错误日志记录
   - 验证情绪分析日志记录

### 结果报告阶段
- 显示每个测试的通过/失败状态
- 显示总体测试结果统计
- 生成详细的测试报告

## 预期输出

### 成功执行示例

```
========================================
Anonymous Emotion Diary - End-to-End Tests
========================================

Test 1: User Registration Flow
  - Testing username validation...
    ✓ Correctly rejected short username
    ✓ Correctly rejected long username
    ✓ Correctly rejected short password
  - Testing valid registration...
    ✓ User registered successfully: testuser_abc123 (ID: 1)
    ✓ User verified in database
  - Testing duplicate username rejection...
    ✓ Correctly rejected duplicate username
✓ Test 1 PASSED: User Registration Flow

Test 2: User Login Flow
  - Creating test user...
    ✓ Test user created: logintest_def456
  - Testing login with correct credentials...
    ✓ Login successful: logintest_def456
  - Testing login with incorrect password...
    ✓ Correctly rejected incorrect password
  - Testing login with non-existent user...
    ✓ Correctly rejected non-existent user
  - Verifying last login timestamp...
    ✓ Last login timestamp updated: 2024-01-15 10:30:45
✓ Test 2 PASSED: User Login Flow

[... 更多测试输出 ...]

========================================
Test Results: 7/7 tests passed
========================================

Press any key to exit...
```

### 失败执行示例

```
Test 3: Diary Creation Flow
  - Creating test user...
    ✓ Test user created: diarytest_ghi789
  - Testing empty content validation...
    ✓ Correctly rejected empty content
  - Testing content length validation...
    ✓ Correctly rejected oversized content
  - Testing valid diary creation...
    ✗ Diary creation failed
✗ Test 3 FAILED: Diary Creation Flow

[... 其他测试 ...]

========================================
Test Results: 6/7 tests passed
========================================
```

## 测试数据

### 创建的测试数据

测试会创建以下数据：
- 多个测试用户账户 (用户名格式: `testtype_randomid`)
- 多篇测试日记
- 调试、错误和情绪分析日志

### 数据位置

- **数据库**: `AnonymousEmotionDiary.db` (SQLite)
- **日志文件**: `Logs/` 目录
  - `debug.log` - 调试日志
  - `error.log` - 错误日志
  - `emotion_analysis.log` - 情绪分析日志

### 清理测试数据

如需清理测试数据：

```bash
# 删除数据库文件
rm AnonymousEmotionDiary.db

# 删除日志文件
rm -r Logs/

# 重新启动应用以重新初始化
```

## 故障排除

### 问题 1：数据库连接失败

**症状**: 测试失败，显示"Database connection failed"

**解决方案**:
1. 确保 `AnonymousEmotionDiary.db` 文件不被其他进程锁定
2. 删除数据库文件并重新运行测试
3. 检查文件系统权限

### 问题 2：日志文件写入失败

**症状**: 测试 7 失败，显示"Log file not created"

**解决方案**:
1. 确保 `Logs/` 目录存在或可创建
2. 检查文件系统权限
3. 确保磁盘空间充足

### 问题 3：情绪分析失败

**症状**: 测试 3 或 4 失败，显示"Emotion analysis failed"

**解决方案**:
1. 这是正常的，如果 LLM API 不可用
2. 系统会自动回退到关键词识别
3. 检查 `ConfigurationHelper.LLMAPIEndpoint` 配置

### 问题 4：测试超时

**症状**: 测试挂起或超时

**解决方案**:
1. 检查 LLM API 连接
2. 增加 `ConfigurationHelper.LLMAPITimeout` 值
3. 检查网络连接

## 性能指标

### 预期执行时间

- **总体**: 5-30 秒 (取决于 LLM API 响应)
- **用户认证测试**: < 1 秒
- **日记操作测试**: 1-5 秒
- **情绪分析测试**: 2-20 秒 (取决于 API)
- **日志系统测试**: < 1 秒

### 资源使用

- **内存**: < 100 MB
- **磁盘**: < 10 MB (包括数据库和日志)
- **网络**: 仅在情绪分析时使用 (如果启用 LLM API)

## 集成到 CI/CD

### GitHub Actions 示例

```yaml
name: Run End-to-End Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '6.0.x'
      - run: dotnet build AnonymousEmotionDiary/AnonymousEmotionDiary.csproj
      - run: dotnet run --project AnonymousEmotionDiary/AnonymousEmotionDiary.csproj -- TestRunner
```

## 测试覆盖范围

### 覆盖的需求

- ✓ 需求 1: 用户认证与账户管理 (1.1-1.6)
- ✓ 需求 2: 日记发布与存储 (2.1-2.5)
- ✓ 需求 3: 情绪检测与分析 (3.1-3.4)
- ✓ 需求 4: 高风险情绪预警 (4.1-4.4)
- ✓ 需求 5: 日记查看与历史记录 (5.1-5.5)
- ✓ 需求 6: 系统日志与分析 (6.1-6.4)

### 覆盖的功能

- ✓ 用户注册和登录
- ✓ 日记创建、查看、删除
- ✓ 情绪分析和高风险检测
- ✓ 日志记录和文件管理
- ✓ 数据库操作
- ✓ 错误处理和验证

## 下一步

1. **运行测试**: 按照上述方式执行测试
2. **验证结果**: 确保所有 7 个测试都通过
3. **检查日志**: 查看生成的日志文件验证记录
4. **用户测试**: 如果所有测试通过，可以进行用户验收测试
5. **部署**: 准备应用部署

## 支持

如有问题或需要帮助，请参考：
- 需求文档: `.kiro/specs/anonymous-emotion-diary/requirements.md`
- 设计文档: `.kiro/specs/anonymous-emotion-diary/design.md`
- 测试报告: `.kiro/specs/anonymous-emotion-diary/END_TO_END_TEST_REPORT.md`
