# 🚀 КАК ДЕРЖАТЬ СЕРВЕР ВСЕГДА ВКЛЮЧЕННЫМ

## 🔴 Проблема
Сервер останавливается когда:
- Закрываешь VS Code
- Жмешь Ctrl+C в терминале
- Происходит ошибка в приложении
- Закрывается окно терминала

---

## ✅ РЕШЕНИЕ 1: Запуск в отдельном окне (рекомендуется для разработки)

### Создай bat-файл для быстрого запуска:

**Файл:** `start-server.bat` (в корне проекта)
```batch
@echo off
title QIMy ERP Server
cd /d "%~dp0"
echo Starting QIMy ERP Server...
echo Server will be available at: http://localhost:5204
echo.
echo Press Ctrl+C to stop
dotnet run --project src/QIMy.Web/QIMy.Web.csproj
pause
```

**Использование:**
1. Двойной клик на `start-server.bat`
2. Окно остается открытым - сервер работает
3. Можешь закрыть VS Code - сервер продолжит работать
4. Чтобы остановить - закрой окно или Ctrl+C

---

## ✅ РЕШЕНИЕ 2: PowerShell скрипт (более гибко)

**Файл:** `Start-QImyServer.ps1` (в корне проекта)
```powershell
# QIMy ERP Server Startup Script
param(
    [switch]$Background,    # Запуск в фоне без окна
    [switch]$NewWindow      # Запуск в новом окне
)

$projectPath = "src/QIMy.Web/QIMy.Web.csproj"
$serverUrl = "http://localhost:5204"

Write-Host "🚀 Starting QIMy ERP Server..." -ForegroundColor Cyan
Write-Host "📍 Project: $projectPath" -ForegroundColor Yellow
Write-Host "🌐 URL: $serverUrl" -ForegroundColor Green
Write-Host ""

if ($Background) {
    # Запуск в фоне без окна
    Write-Host "⚙️ Starting in background..." -ForegroundColor Magenta
    Start-Process "dotnet" -ArgumentList "run --project $projectPath" `
        -WindowStyle Hidden -PassThru
    Write-Host "✅ Server started in background" -ForegroundColor Green
    Write-Host "💡 To stop: Get-Process dotnet | Stop-Process" -ForegroundColor Yellow
}
elseif ($NewWindow) {
    # Запуск в новом окне
    Write-Host "🪟 Starting in new window..." -ForegroundColor Magenta
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", `
        "cd '$PWD'; Write-Host '🚀 QIMy Server Running' -ForegroundColor Green; dotnet run --project $projectPath" `
        -WindowStyle Normal
    Write-Host "✅ Server started in new window" -ForegroundColor Green
}
else {
    # Запуск в текущем окне
    Write-Host "▶️ Starting in current window..." -ForegroundColor Magenta
    Write-Host "⚠️ Press Ctrl+C to stop" -ForegroundColor Yellow
    Write-Host ""
    dotnet run --project $projectPath
}
```

**Использование:**
```powershell
# Обычный запуск (в текущем окне)
.\Start-QImyServer.ps1

# Запуск в новом окне (можно закрыть VS Code)
.\Start-QImyServer.ps1 -NewWindow

# Запуск в фоне (совсем невидимый)
.\Start-QImyServer.ps1 -Background

# Остановить фоновый процесс
Get-Process dotnet | Where-Object {$_.CommandLine -like "*QIMy*"} | Stop-Process
```

---

## ✅ РЕШЕНИЕ 3: Windows Service (для продакшена)

### Установка как Windows Service (работает всегда, даже после перезагрузки):

**1. Установи NSSM (Non-Sucking Service Manager):**
```powershell
# Через Chocolatey
choco install nssm

# Или скачай: https://nssm.cc/download
```

**2. Создай службу:**
```powershell
# Открой PowerShell от Администратора
cd C:\Projects\QIMy

# Создай службу
nssm install QImyERP "C:\Program Files\dotnet\dotnet.exe"

# Настрой параметры
nssm set QImyERP AppDirectory "C:\Projects\QIMy"
nssm set QImyERP AppParameters "run --project src/QIMy.Web/QIMy.Web.csproj"
nssm set QImyERP DisplayName "QIMy ERP Server"
nssm set QImyERP Description "QIMy ERP SaaS Accounting System"
nssm set QImyERP Start SERVICE_AUTO_START

# Запусти службу
nssm start QImyERP
```

**Управление службой:**
```powershell
# Статус
nssm status QImyERP

# Остановить
nssm stop QImyERP

# Перезапустить
nssm restart QImyERP

# Удалить службу
nssm remove QImyERP confirm
```

**Теперь сервер:**
- ✅ Запускается автоматически при включении ПК
- ✅ Работает в фоне всегда
- ✅ Перезапускается при сбое
- ✅ Не зависит от VS Code или терминала

---

## ✅ РЕШЕНИЕ 4: Docker (изолированно)

**Файл:** `docker-compose.yml` (уже есть в проекте?)
```yaml
version: '3.8'
services:
  qimy-web:
    build: .
    ports:
      - "5204:80"
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

**Запуск:**
```powershell
docker-compose up -d
```

**Преимущества:**
- ✅ Работает в контейнере (не мешает другим проектам)
- ✅ Автоматический перезапуск
- ✅ Легко развернуть на любом сервере

---

## 🎯 ЧТО ВЫБРАТЬ?

### Для разработки (сейчас):
👉 **Решение 1 или 2** - bat/PowerShell скрипт с новым окном

### Для тестирования на локальном ПК:
👉 **Решение 3** - Windows Service (NSSM)

### Для продакшена:
👉 **Azure App Service** (уже настроен для проекта)
👉 **Docker** на сервере

---

## 🔍 Проверка статуса сервера

### PowerShell команды:
```powershell
# Проверить запущен ли
Get-Process dotnet | Where-Object {$_.CommandLine -like "*QIMy*"}

# Проверить порт 5204
Test-NetConnection -ComputerName localhost -Port 5204

# Открыть в браузере
Start-Process "http://localhost:5204"

# Остановить все dotnet процессы
Get-Process dotnet | Stop-Process -Force
```

---

## 💡 БЫСТРЫЙ СТАРТ (ПРЯМО СЕЙЧАС)

**Способ 1: Двойной клик на bat-файл**
1. Создай `start-server.bat` (см. выше)
2. Двойной клик
3. Готово!

**Способ 2: Одна команда PowerShell**
```powershell
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd C:\Projects\QIMy; dotnet run --project src/QIMy.Web/QIMy.Web.csproj" -WindowStyle Normal
```

---

## ⚠️ ВАЖНО

### Почему сервер падал:
Проверь логи: `session-logs/web.log` или `session-logs/api.log`

### Типичные причины:
1. **Порт занят** - другое приложение использует 5204
   ```powershell
   # Проверить кто использует порт
   netstat -ano | findstr :5204
   ```

2. **Ошибка в коде** - сервер крашится при запуске
   ```powershell
   # Смотри логи
   dotnet run --project src/QIMy.Web/QIMy.Web.csproj --verbosity detailed
   ```

3. **База данных недоступна** - проверь connection string

---

**Создано:** 2026-01-29
**Для проекта:** QIMy ERP
