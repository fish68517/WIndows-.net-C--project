# Windows 运行测试 - 完整步骤

## 前置条件检查

### 1. 检查 .NET 是否已安装

打开 PowerShell 或 CMD，运行：
```bash
dotnet --version
```

**预期输出**: 显示 .NET 版本号（如 6.0.x 或 7.0.x）

**如果显示 "command not found"**:
- 需要安装 .NET SDK
- 下载地址: https://dotnet.microsoft.com/download
- 选择 .NET 6 或更高版本
- 安装后重启终端

---

## 运行测试 - 完整步骤

### 步骤 1: 打开终端

**选项 A - PowerShell (推荐)**
- 按 `Win + X`，选择 "Windows PowerShell" 或 "Windows Terminal"

**选项 B - CMD**
- 按 `Win + R`，输入 `cmd`，按 Enter

**选项 C - Git Bash**
- 在项目文件夹右键，选择 "Git Bash Here"

---

### 步骤 2: 进入项目目录

```bash
cd AnonymousEmotionDiary
```

**验证**: 你应该看到类似这样的路径：
```
D:\Acode\Android\complete\AA项目归档\作业-windows.Net项目\AnonymousEmotionDiary\AnonymousEmotionDiary>
```

---

### 步骤 3: 编译项目

```bash
dotnet build
```

**预期输出**:
```
Build started...
...
Build succeeded.
```

**如果失败**: 检查是否有编译错误，通常是缺少 NuGet 包

---

### 步骤 4: 运行测试

```bash
dotnet run -- TestRunner
```

**预期输出**:
```
========================================
Anonymous Emotion Diary - End-to-End Tests
========================================

Test 1: User Registration Flow
  ✓ Test 1 PASSED: User Registration Flow

Test 2: User Login Flow
  ✓ Test 2 PASSED: User Login Flow

[... 更多测试 ...]

========================================
Test Results: 7/7 tests passed
========================================

Press any key to exit...
```

---

## 常见错误及解决方案

### 错误 1: "donet: command not found"

**原因**: 输入错误，少了一个 't'

**解决**: 确保输入 `dotnet`（不是 `donet`）

```bash
# ❌ 错误
donet run

# ✓ 正确
dotnet run
```

---

### 错误 2: "dotnet: command not found"

**原因**: .NET SDK 未安装或未添加到 PATH

**解决**:
1. 下载 .NET SDK: https://dotnet.microsoft.com/download
2. 安装 .NET 6 或更高版本
3. 重启终端
4. 验证: `dotnet --version`

---

### 错误 3: "The project file could not be loaded"

**原因**: 不在正确的目录

**解决**:
```bash
# 确保你在 AnonymousEmotionDiary 目录
cd AnonymousEmotionDiary

# 验证目录内容
dir
# 应该看到: AnonymousEmotionDiary.csproj, Program.cs 等文件
```

---

### 错误 4: "Build failed"

**原因**: 缺少 NuGet 包或编译错误

**解决**:
```bash
# 清理并重新构建
dotnet clean
dotnet restore
dotnet build
```

---

### 错误 5: "Database connection failed"

**原因**: 数据库文件被锁定或权限问题

**解决**:
```bash
# 删除旧数据库
del AnonymousEmotionDiary.db

# 重新运行测试
dotnet run -- TestRunner
```

---

## 快速参考

### 完整命令序列

```bash
# 1. 进入项目目录
cd AnonymousEmotionDiary

# 2. 编译项目
dotnet build

# 3. 运行测试
dotnet run -- TestRunner
```

### 一行命令

```bash
cd AnonymousEmotionDiary && dotnet build && dotnet run -- TestRunner
```

---

## 运行应用（不运行测试）

如果你想运行应用而不是测试：

```bash
cd AnonymousEmotionDiary
dotnet run
```

这会启动 WinForms 应用的主窗口。

---

## 验证测试结果

### 成功标志
- 看到 "Test Results: 7/7 tests passed"
- 所有 7 个测试都显示 ✓ PASSED

### 检查生成的文件

测试完成后，会生成以下文件：

```
AnonymousEmotionDiary/
├── AnonymousEmotionDiary.db    # SQLite 数据库
└── Logs/
    ├── debug.log               # 调试日志
    ├── error.log               # 错误日志
    └── emotion_analysis.log    # 情绪分析日志
```

---

## 清理测试数据

如果需要重新开始，删除这些文件：

```bash
# 删除数据库
del AnonymousEmotionDiary.db

# 删除日志目录
rmdir /s /q Logs

# 重新运行测试
dotnet run -- TestRunner
```

---

## 使用 Visual Studio (可选)

如果你有 Visual Studio 2022：

1. 打开 `AnonymousEmotionDiary.sln`
2. 在解决方案资源管理器中找到 `TestRunner.cs`
3. 右键 → 设置为启动对象
4. 按 F5 运行

---

## 故障排除检查清单

- [ ] 已安装 .NET SDK (检查: `dotnet --version`)
- [ ] 在正确的目录 (检查: `dir` 看到 .csproj 文件)
- [ ] 输入正确的命令 (检查: `dotnet` 不是 `donet`)
- [ ] 项目可以编译 (检查: `dotnet build` 成功)
- [ ] 测试可以运行 (检查: `dotnet run -- TestRunner` 成功)

---

## 需要帮助？

如果仍然有问题，请提供：
1. 完整的错误信息
2. 你运行的命令
3. `dotnet --version` 的输出
4. 你所在的目录路径

---

**记住**: 最常见的错误就是输入 `donet` 而不是 `dotnet` 😊
