@echo off
title QIMy ERP Server
cd /d "%~dp0"
echo.
echo ========================================
echo    QIMy ERP Server
echo ========================================
echo.
echo Starting server...
echo Server will be available at: http://localhost:5204
echo.
echo Press Ctrl+C to stop
echo.
dotnet run --project src/QIMy.Web/QIMy.Web.csproj
pause
