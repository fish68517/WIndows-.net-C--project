@echo off
setlocal

if not exist build mkdir build

gcc src/main.c ^
    src/app_paths.c ^
    src/csv_loader.c ^
    src/preprocess.c ^
    src/autoencoder.c ^
    src/detector.c ^
    src/gui.c ^
    src/logger.c ^
    -O2 ^
    -std=c11 ^
    -Wall ^
    -Wextra ^
    -static ^
    -static-libgcc ^
    -mwindows ^
    -o build/network_anomaly_detector.exe ^
    -lm ^
    -luser32 ^
    -lgdi32 ^
    -lshell32 ^
    -lole32

if errorlevel 1 (
    echo Build failed.
    exit /b 1
)

echo Build finished: build\network_anomaly_detector.exe
