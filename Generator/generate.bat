@echo off
echo #ifdef __arm__
generator template.json .\path\to\script.json
echo #elif __aarch64__
echo.
generator template.json .\path\to\script.json
echo #endif
pause>nul