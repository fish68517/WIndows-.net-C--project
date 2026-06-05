$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Force build | Out-Null

gcc src/main.c `
    src/app_paths.c `
    src/csv_loader.c `
    src/preprocess.c `
    src/autoencoder.c `
    src/detector.c `
    src/gui.c `
    src/logger.c `
    -O2 `
    -std=c11 `
    -Wall `
    -Wextra `
    -static `
    -static-libgcc `
    -mwindows `
    -o build/network_anomaly_detector.exe `
    -lm `
    -luser32 `
    -lgdi32 `
    -lshell32 `
    -lole32

Write-Host "Build finished: build/network_anomaly_detector.exe"
