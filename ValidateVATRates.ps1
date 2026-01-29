# Проверка справочников Personen Index.xlsx на ошибки
# Эксперт-анализ ставок НДС по странам ЕС на 2026г

## ОФИЦИАЛЬНЫЕ СТАВКИ НДС В ЕС НА 2026Г
## (Источник: European Commission, VAT Directive 2006/112/EC)

$officialRates = @{
    "AT" = 20  # Австрия
    "BE" = 21  # Бельгия
    "BG" = 20  # Болгария
    "HR" = 25  # Хорватия
    "CY" = 19  # Кипр
    "CZ" = 21  # Чехия
    "DK" = 25  # Дания
    "EE" = 20  # Эстония
    "FI" = 24  # Финляндия
    "FR" = 20  # Франция
    "DE" = 19  # Германия
    "EL" = 24  # Греция (EL не ELY)
    "HU" = 27  # Венгрия
    "IE" = 23  # Ирландия
    "IT" = 22  # Италия
    "LV" = 21  # Латвия
    "LT" = 21  # Литва
    "LU" = 17  # Люксембург
    "MT" = 18  # Мальта
    "NL" = 21  # Нидерланды
    "PL" = 23  # Польша
    "PT" = 23  # Португалия
    "RO" = 19  # Румыния
    "SK" = 20  # Словакия
    "SI" = 22  # Словения
    "ES" = 21  # Испания
    "SE" = 25  # Швеция
    "UK" = 20  # Великобритания (не ЕС, но в таблице)
}

# Данные из скриншота EU-Rate
$excelData = @{
    "AT" = 20
    "BE" = 21
    "BG" = 20
    "HR" = 25
    "CY" = 19
    "CZ" = 21
    "DK" = 25
    "EE" = 20
    "FI" = 24
    "FR" = 20
    "DE" = 19
    "EL" = 24  # предполагаю, что это Греция
    "HU" = 27
    "IE" = 23
    "IT" = 22
    "LV" = 21
    "LT" = 21
    "LU" = 17
    "MT" = 18
    "NL" = 21
    "PL" = 23
    "PT" = 23
    "RO" = 19
    "SK" = 20
    "SI" = 22
    "ES" = 21
    "SE" = 25
    "UK" = 20
}

Write-Host "=== ПРОВЕРКА СТАВОК НДС В EU-RATE ===" -ForegroundColor Green
Write-Host ""
Write-Host "Сравнение с официальными ставками EC на 2026г" -ForegroundColor Cyan
Write-Host ""

$errors = @()
$correct = @()

foreach ($code in $officialRates.Keys | Sort-Object) {
    $official = $officialRates[$code]
    $inExcel = $excelData[$code]

    if ($inExcel -eq $null) {
        Write-Host "MISSING: $code (official $official%)" -ForegroundColor Yellow
        $errors += "Code $code missing"
    }
    elseif ($inExcel -ne $official) {
        Write-Host "ERROR: $code - Excel=$inExcel%, Official=$official%" -ForegroundColor Red
        $errors += "Code $code mismatch (Excel=$inExcel%, Official=$official%)"
    }
    else {
        $correct += $code
    }
}

Write-Host ""
Write-Host "VALIDATION SUMMARY" -ForegroundColor Cyan
Write-Host "Correct rates: $($correct.Count)" -ForegroundColor Green
Write-Host "Errors: $($errors.Count)" -ForegroundColor Red
Write-Host ""

if ($errors.Count -gt 0) {
    Write-Host "Found errors:" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "  * $_" }
}
else {
    Write-Host "All VAT rates are correct!" -ForegroundColor Green
}
