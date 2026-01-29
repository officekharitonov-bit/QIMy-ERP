# Подготовка CSV для импорта поставщиков BKHA GmbH

$pkPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\PK 2025 - BKHA GmbH - 26-01-2026.csv"
$outputPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Suppliers_BKHA_Import.csv"

Write-Host "=== ПОДГОТОВКА ПОСТАВЩИКОВ BKHA GmbH ===" -ForegroundColor Cyan

# Читаем файл построчно
$allLines = Get-Content $pkPath -Encoding UTF8

Write-Host "`nВсего строк: $($allLines.Count)" -ForegroundColor Yellow

# Берём заголовок из строки 2 и данные начиная со строки 3
$header = $allLines[1]
$dataLines = $allLines[2..($allLines.Count - 1)]

# Парсим данные вручную
$suppliers = @()
foreach ($line in $dataLines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }

    $fields = $line -split ';'

    # Kto-Nr в индексе 1
    $ktoNr = $fields[1]

    # Фильтр поставщиков (230xxx)
    if ($ktoNr -match '^23') {
        $suppliers += [PSCustomObject]@{
            'Externe KontoNr'            = $fields[0]
            'Kto-Nr'                     = $fields[1]
            'Nachname'                   = $fields[2]
            'Freifeld 06'                = $fields[3]
            'Straße'                     = $fields[4]
            'Plz'                        = $fields[5]
            'Ort'                        = $fields[6]
            'Währung'                    = $fields[7]
            'ZZiel'                      = $fields[8]
            'SktoProz1'                  = $fields[9]
            'SktoTage1'                  = $fields[10]
            'UID-Nummer'                 = $fields[11]
            'Freifeld 11'                = $fields[12]
            'Lief-Vorschlag Gegenkonto'  = $fields[13]
            'Freifeld 04'                = $fields[14]
            'Freifeld 05'                = $fields[15]
            'Kundenvorschlag Gegenkonto' = $fields[16]
            'Freifeld 02'                = $fields[17]
            'Freifeld 03'                = $fields[18]
            'Filial-Nr'                  = $fields[19]
            'Land-Nr'                    = $fields[20]
            'IBAN'                       = $fields[21]
        }
    }
}

Write-Host "Найдено поставщиков (230xxx): $($suppliers.Count)" -ForegroundColor Green

# Показываем что нашли
if ($suppliers.Count -gt 0) {
    Write-Host "`n=== НАЙДЕННЫЕ ПОСТАВЩИКИ ===" -ForegroundColor Cyan
    foreach ($supplier in $suppliers) {
        $country = switch ($supplier.'Land-Nr') {
            '1' { 'Österreich' }
            '4' { 'Belgien' }
            '10' { 'Spanien' }
            '16' { 'Ungarn' }
            '20' { 'Polen' }
            '24' { 'Bulgarien' }
            '27' { 'Lettland' }
            '246' { 'Zypern' }
            default { "Land: $($supplier.'Land-Nr')" }
        }
        Write-Host "$($supplier.'Kto-Nr') - $($supplier.'Nachname') ($country)" -ForegroundColor White
    }

    # Экспортируем
    $suppliers | Export-Csv -Path $outputPath -Delimiter ';' -Encoding UTF8 -NoTypeInformation

    Write-Host "`n✅ Экспортировано в: $outputPath" -ForegroundColor Green
    Write-Host "`nТеперь можно импортировать через:" -ForegroundColor Cyan
    Write-Host "  /ER/Suppliers/Import" -ForegroundColor White
}
else {
    Write-Host "`n⚠️  Поставщики не найдены!" -ForegroundColor Yellow
}
