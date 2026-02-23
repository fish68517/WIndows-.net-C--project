# 快速开始 - 端到端测试

## 30 秒快速开始

### 步骤 1: 打开项目
```bash
cd AnonymousEmotionDiary
```

### 步骤 2: 编译项目
```bash
dotnet build
```

### 步骤 3: 运行测试
```bash
dotnet run -- TestRunner
```

### 步骤 4: 查看结果
```
========================================
Anonymous Emotion Diary - End-to-End Tests
========================================

Test 1: User Registration Flow
  ✓ Test 1 PASSED: User Registration Flow

Test 2: User Login Flow
  ✓ Test 2 PASSED: User Login Flow

Test 3: Diary Creation Flow
  ✓ Test 3 PASSED: Diary Creation Flow

Test 4: High-Risk Emotion Detection
  ✓ Test 4 PASSED: High-Risk Emotion Detection

Test 5: Diary Viewing Flow
  ✓ Test 5 PASSED: Diary Viewing Flow

Test 6: Diary Deletion Flow
  ✓ Test 6 PASSED: Diary Deletion Flow

Test 7: Logging System
  ✓ Test 7 PASSED: Logging System

========================================
Test Results: 7/7 tests passed
========================================
```

## 测试验证的功能

| 功能 | 测试 | 状态 |
|------|------|------|
| 用户注册 | Test 1 | ✓ |
| 用户登录 | Test 2 | ✓ |
| 日记创建 | Test 3 | ✓ |
| 情绪分析 | Test 3, 4 | ✓ |
| 高风险检测 | Test 4 | ✓ |
| 日记查看 | Test 5 | ✓ |
| 日记删除 | Test 6 | ✓ |
| 日志记录 | Test 7 | ✓ |

## 测试覆盖的需求

✓ 需求 1: 用户认证与账户管理  
✓ 需求 2: 日记发布与存储  
✓ 需求 3: 情绪检测与分析  
✓ 需求 4: 高风险情绪预警  
✓ 需求 5: 日记查看与历史记录  
✓ 需求 6: 系统日志与分析  

## 测试输出文件

运行测试后会生成以下文件：

- `AnonymousEmotionDiary.db` - SQLite 数据库
- `Logs/debug.log` - 调试日志
- `Logs/error.log` - 错误日志
- `Logs/emotion_analysis.log` - 情绪分析日志

## 清理测试数据

```bash
# 删除数据库
rm AnonymousEmotionDiary.db

# 删除日志
rm -r Logs/
```

## 常见问题

**Q: 测试需要多长时间?**  
A: 通常 5-30 秒，取决于 LLM API 响应时间。

**Q: 如果 LLM API 不可用怎么办?**  
A: 系统会自动回退到关键词识别，测试仍会通过。

**Q: 测试会修改我的数据吗?**  
A: 不会。测试使用独立的测试数据库。

**Q: 如何查看详细的测试报告?**  
A: 查看 `END_TO_END_TEST_REPORT.md` 文件。

## 下一步

1. ✓ 运行测试验证所有功能
2. 启动应用进行手动测试
3. 进行用户验收测试 (UAT)
4. 部署到生产环境

---

**所有测试通过 = 应用已准备好部署** ✓
