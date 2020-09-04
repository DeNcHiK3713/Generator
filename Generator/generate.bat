@echo off
echo #ifdef __arm__
generator template.json .\path\to\script.json .\path\to\libil2cpp.so ARM
echo #elif __aarch64__
echo.
generator template.json .\path\to\script.json .\path\to\libil2cpp.so ARM64
echo #endif
pause>nul