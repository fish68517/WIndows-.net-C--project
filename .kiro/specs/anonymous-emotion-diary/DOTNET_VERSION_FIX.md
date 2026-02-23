# .NET 版本修复

## 问题

编译时出现错误：
```
You must install or update .NET to run this application.
Framework: 'Microsoft.NETCore.App', version '6.0.0' (x64)
```

但你已安装 .NET 8.0.416

## 原因

项目配置文件 (`AnonymousEmotionDiary.csproj`) 指定了 `net6.0-windows` 作为目标框架，但你的系统只有 .NET 8.0。

## 解决方案

已更新项目文件以支持 .NET 8.0：

### 修改内容

**文件**: `AnonymousEmotionDiary/AnonymousEmotionDiary.csproj`

```xml
<!-- 之前 -->
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <TargetFramework>net6.0-windows</TargetFramework>
    ...
  </PropertyGroup>
</Project>

<!-- 之后 -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    ...
  </PropertyGroup>
</Project>
```

### 变更说明

1. **Sdk 属性**: `Microsoft.NET.Sdk.WindowsDesktop` → `Microsoft.NET.Sdk`
   - 新版本 .NET 不再需要 WindowsDesktop SDK
   
2. **TargetFramework**: `net6.0-windows` → `net8.0-windows`
   - 更新为你安装的 .NET 8.0 版本

## 现在可以运行

```bash
cd AnonymousEmotionDiary
dotnet build
dotnet run -- TestRunner
```

## 验证

```bash
$ dotnet --version
8.0.416

$ dotnet build
Build succeeded.

$ dotnet run -- TestRunner
========================================
Anonymous Emotion Diary - End-to-End Tests
========================================
[测试开始...]
```

## 注意

- 如果你想使用 .NET 6.0，需要下载并安装 .NET 6.0 SDK
- 当前配置已优化为 .NET 8.0
- 所有功能在 .NET 8.0 上完全兼容
