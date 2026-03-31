@echo off
echo #ifdef __arm__
generator template.json .\path\to\script.json .\path\to\libil2cpp.so
echo #elif __aarch64__
echo.
generator template.json .\path\to\script.json .\path\to\libil2cpp.so
echo #endif
pause>nul