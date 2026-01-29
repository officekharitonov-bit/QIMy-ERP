# Import Personen Index data into QIMy database
# This script requires the API to be running

$excelPath = "c:\Projects\QIMy\tabellen\Personen index.xlsx"
$apiUrl = "http://localhost:5000"

Write-Host "=== Personen Index Import ===" -ForegroundColor Cyan
Write-Host "Excel file: $excelPath"
Write-Host "API URL: $apiUrl"
Write-Host ""

# Check if Excel file exists
if (-not (Test-Path $excelPath)) {
    Write-Host "ERROR: Excel file not found: $excelPath" -ForegroundColor Red
    exit 1
}

# Check if API is running
try {
    $health = Invoke-RestMethod -Uri "$apiUrl/health" -Method Get -ErrorAction Stop
    Write-Host "✓ API is running" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: API is not running. Start it with: dotnet run --project src/QIMy.API/QIMy.API.csproj" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting import..." -ForegroundColor Yellow

# Call import endpoint (we'll create this next)
try {
    $form = @{
        file = Get-Item $excelPath
    }

    $result = Invoke-RestMethod -Uri "$apiUrl/api/import/personen-index" -Method Post -Form $form -ErrorAction Stop

    Write-Host ""
    Write-Host "=== Import Results ===" -ForegroundColor Cyan
    Write-Host "Countries Imported: $($result.countriesImported)"
    Write-Host "Countries Updated: $($result.countriesUpdated)"
    Write-Host "EU Data Imported: $($result.euDataImported)"
    Write-Host "EU Data Updated: $($result.euDataUpdated)"
    Write-Host ""

    if ($result.errors -and $result.errors.Count -gt 0) {
        Write-Host "ERRORS:" -ForegroundColor Red
        foreach ($error in $result.errors) {
            Write-Host "  ❌ $error" -ForegroundColor Red
        }
        exit 1
    }

    Write-Host "✓ Import completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host $result.summary

}
catch {
    Write-Host "ERROR: Import failed" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
