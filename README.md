# 基于深度学习的网络异常流量检测系统

本项目使用 C 语言实现轻量级自编码器，用于网络异常流量检测。交付方式采用 MinGW-w64 编译 Windows exe。

## 编译

在 PowerShell 中执行：

```powershell
.\build.ps1
```

如果 PowerShell 禁止执行脚本，可以改用：

```powershell
.\build.bat
```

或直接执行：

```powershell
gcc src/main.c src/app_paths.c src/csv_loader.c src/preprocess.c src/autoencoder.c src/detector.c src/gui.c src/logger.c -O2 -std=c11 -Wall -Wextra -static -static-libgcc -mwindows -o build/network_anomaly_detector.exe -lm -luser32 -lgdi32 -lshell32 -lole32
```

## 运行

双击运行：

```text
build/network_anomaly_detector.exe
```

双击后会打开图形界面，可点击“训练模型”“测试模型”“实时检测”“查看日志”“输出目录”。

GUI 默认加载：

```text
data/processed/UNSW-NB15-demo/
```

也可以点击“选择数据目录”加载：

```text
data/processed/UNSW-NB15/
data/raw/UNSW-NB15/
```

其中 `processed` 目录是带表头、逗号分隔的标准 CSV，推荐演示和正式运行使用；`raw` 目录是原始下载文件，实际为无表头 TSV，程序已支持自动识别。

训练模型：

```powershell
.\build\network_anomaly_detector.exe train data/train_small.csv
```

测试模型：

```powershell
.\build\network_anomaly_detector.exe test data/test_small.csv
```

模拟实时检测：

```powershell
.\build\network_anomaly_detector.exe detect data/realtime_sample.csv
```

## 输出文件

```text
model/autoencoder_model.bin      自编码器模型
model/normalize_params.txt       归一化参数
output/train_log.txt             训练日志
output/detect_result.csv         检测结果
output/alarm_log.txt             异常报警日志
output/test_metrics.txt          测试评价指标
output/app.log                   程序启动和运行诊断日志
build/network_anomaly_detector.exe Windows 可执行程序
```

## 双击 exe 的说明

当前程序已支持直接双击 `build/network_anomaly_detector.exe` 打开 GUI。程序会自动识别自己位于 `build` 目录，并把工作目录切换到项目根目录，因此可以直接读取 `data/model/output`。
