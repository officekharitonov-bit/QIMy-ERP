# Remove duplicate German keyboard layout
# Keep only one German keyboard (de-AT or de-DE)

Write-Host "=== Удаление лишнего немецкого расклада ===" -ForegroundColor Green

# Get current language list
$list = Get-WinUserLanguageList
Write-Host "`nТекущие языки:" -ForegroundColor Cyan
$list | ForEach-Object { Write-Host "  - $($_.LanguageTag)" }

# Count German keyboards
$germanCount = ($list | Where-Object { $_.LanguageTag -like "de-*" }).Count
Write-Host "`nНайдено немецких раскладок: $germanCount" -ForegroundColor Yellow

if ($germanCount -le 1) {
    Write-Host "✅ Только один немецкий или нет немецкого. Никаких действий не требуется." -ForegroundColor Green
    exit 0
}

# Remove duplicate German, keep only de-AT (more commonly used in Austria/Switzerland)
# If de-AT doesn't exist, keep de-DE
$newList = @()
$hasGerman = $false

foreach ($lang in $list) {
    if ($lang.LanguageTag -like "de-*") {
        if (!$hasGerman) {
            # Keep first German (prioritize de-AT, then de-DE)
            if ($lang.LanguageTag -eq "de-AT" -or $lang.LanguageTag -eq "de-DE") {
                $newList += $lang
                $hasGerman = $true
                Write-Host "✅ Сохранён: $($lang.LanguageTag)" -ForegroundColor Green
            }
        } else {
            Write-Host "❌ Удалён лишний: $($lang.LanguageTag)" -ForegroundColor Red
        }
    } else {
        # Keep all non-German languages
        $newList += $lang
    }
}

# Apply new list
try {
    Set-WinUserLanguageList $newList -Force
    Write-Host "`n✅ Языковые параметры успешно обновлены!" -ForegroundColor Green
    Write-Host "`nНовые языки:" -ForegroundColor Cyan
    Get-WinUserLanguageList | ForEach-Object { Write-Host "  - $($_.LanguageTag)" }
    Write-Host "`n⚠️  Необходима переавторизация для полного применения изменений." -ForegroundColor Yellow
} catch {
    Write-Host "❌ Ошибка при применении изменений: $_" -ForegroundColor Red
    exit 1
}
