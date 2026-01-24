$officialRates = @{
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
    "EL" = 24
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
    "EL" = 24
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

Write-Host "VAT RATES VALIDATION" -ForegroundColor Green
Write-Host ""

$errors = 0
$correct = 0

foreach ($code in $officialRates.Keys | Sort-Object) {
    $official = $officialRates[$code]
    $inExcel = $excelData[$code]
    
    if ($inExcel -eq $null) {
        Write-Host "MISSING: $code" -ForegroundColor Yellow
        $errors++
    } elseif ($inExcel -ne $official) {
        Write-Host "ERROR: $code - Excel=$inExcel%, Official=$official%" -ForegroundColor Red
        $errors++
    } else {
        $correct++
    }
}

Write-Host ""
Write-Host "Results:" -ForegroundColor Cyan
Write-Host "Correct: $correct" -ForegroundColor Green
Write-Host "Errors: $errors" -ForegroundColor Red
