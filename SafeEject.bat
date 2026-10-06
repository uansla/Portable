@echo off
chcp 65001 >nul
title SafeEject Portable - USB TF Card Eject Tool


echo.
echo =========================================
echo        SafeEject Portable
echo        USB / TF / HDD Safe Remove
echo =========================================
echo.


powershell ^
-NoProfile ^
-ExecutionPolicy Bypass ^
-File "%~dp0SafeEject.ps1"


pause
