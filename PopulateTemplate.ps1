# Populate Template Business (BusinessId=1) with reference data
$ErrorActionPreference = "Stop"

$dbPath = Join-Path $PSScriptRoot "qimy_dev.db"
Write-Host "📂 Database: $dbPath"

# Load SQLite assembly
Add-Type -Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Data.dll"

# Create connection
$connectionString = "Data Source=$dbPath"
$connection = New-Object -TypeName System.Data.Common.DbConnection

Write-Host "❌ SQLite не установлен через ADO.NET. Используйте migration через EF Core!"
Write-Host ""
Write-Host "👉 РЕШЕНИЕ: Добавьте данные через миграцию EF Core или через Web UI после запуска"
Write-Host ""
Write-Host "🔧 Для добавления вручную:"
Write-Host "1. Запустите приложение: dotnet run --project src/QIMy.Web/QIMy.Web.csproj"
Write-Host "2. Зайдите в систему как Шаблон (BusinessId=1)"
Write-Host "3. Добавьте справочники вручную через UI"
Write-Host "4. ИЛИ используйте SQL файл PopulateTemplateBusiness.sql через DB Browser for SQLite"
