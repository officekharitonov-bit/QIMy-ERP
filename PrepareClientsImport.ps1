# Подготовка CSV для импорта клиентов BKHA GmbH

$pkPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\PK 2025 - BKHA GmbH - 26-01-2026.csv"
$outputPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Clients_BKHA_Import.csv"

Write-Host "=== ПОДГОТОВКА КЛИЕНТОВ BKHA GmbH ===" -ForegroundColor Cyan

# Читаем файл построчно
$allLines = Get-Content $pkPath -Encoding UTF8

Write-Host "`nВсего строк: $($allLines.Count)" -ForegroundColor Yellow

# Берём заголовок из строки 2 (индекс 1) и данные начиная со строки 3
$header = $allLines[1]
$dataLines = $allLines[2..($allLines.Count-1)]

Write-Host "Заголовок: $($header.Substring(0, [Math]::Min(100, $header.Length)))..." -ForegroundColor Gray

# Парсим данные вручную
$clients = @()
foreach ($line in $dataLines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    
    $fields = $line -split ';'
    
    # Kto-Nr в индексе 1
    $ktoNr = $fields[1]
    
    # Фильтр клиентов (200xxx)
    if ($ktoNr -match '^200') {
        $clients += [PSCustomObject]@{
            'Externe KontoNr' = $fields[0]
            'Kto-Nr' = $fields[1]
            'Nachname' = $fields[2]
            'Freifeld 06' = $fields[3]
            'Straße' = $fields[4]
            'Plz' = $fields[5]
            'Ort' = $fields[6]
            'Währung' = $fields[7]
            'ZZiel' = $fields[8]
            'SktoProz1' = $fields[9]
            'SktoTage1' = $fields[10]
            'UID-Nummer' = $fields[11]
            'Freifeld 11' = $fields[12]
            'Lief-Vorschlag Gegenkonto' = $fields[13]
            'Freifeld 04' = $fields[14]
            'Freifeld 05' = $fields[15]
            'Kundenvorschlag Gegenkonto' = $fields[16]
            'Freifeld 02' = $fields[17]
            'Freifeld 03' = $fields[18]
            'Filial-Nr' = $fields[19]
            'Land-Nr' = $fields[20]
            'IBAN' = $fields[21]
        }
    }
}

Write-Host "Найдено клиентов (200xxx): $($clients.Count)" -ForegroundColor Green

# Показываем что нашли
if ($clients.Count -gt 0) {
    Write-Host "`n=== НАЙДЕННЫЕ КЛИЕНТЫ ===" -ForegroundColor Cyan
    foreach ($client in $clients) {
        Write-Host "$($client.'Kto-Nr') - $($client.'Nachname')" -ForegroundColor White
    }

    # Экспортируем
    $clients | Export-Csv -Path $outputPath -Delimiter ';' -Encoding UTF8 -NoTypeInformation

    Write-Host "`n✅ Экспортировано в: $outputPath" -ForegroundColor Green
    Write-Host "`nТеперь можно импортировать через:" -ForegroundColor Cyan
    Write-Host "  /AR/Clients/SmartImport" -ForegroundColor White
} else {
    Write-Host "`n⚠️  Клиенты не найдены!" -ForegroundColor Yellow
}
