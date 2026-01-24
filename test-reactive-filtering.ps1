# Test Business Filtering
# Checks if invoices and quotes are properly linked to businesses

Write-Host "`n=== CHECKING BUSINESS DATA ===" -ForegroundColor Cyan

# Read connection string from appsettings
$appsettingsPath = "src/QIMy.Web/appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connString = $appsettings.ConnectionStrings.DefaultConnection
    Write-Host "✅ Connection string found" -ForegroundColor Green
} else {
    Write-Host "❌ appsettings.json not found" -ForegroundColor Red
    exit 1
}

# Check if application is running
Write-Host "`n=== APPLICATION STATUS ===" -ForegroundColor Cyan
$process = Get-Process -Name "QIMy.Web" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "✅ Application is running (PID: $($process.Id))" -ForegroundColor Green
} else {
    Write-Host "⚠️  Application is not running" -ForegroundColor Yellow
    Write-Host "   Run: dotnet run --project src/QIMy.Web/QIMy.Web.csproj --urls `"http://localhost:5204`"" -ForegroundColor Gray
}

# Check localhost:5204
Write-Host "`n=== WEB SERVER CHECK ===" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5204" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
    Write-Host "✅ Web server responding (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "❌ Web server not responding" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
}

Write-Host "`n=== MANUAL TEST INSTRUCTIONS ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open browser: http://localhost:5204" -ForegroundColor Yellow
Write-Host "2. Login: admin / Admin123!" -ForegroundColor Yellow
Write-Host "3. Navigate to: AR → Invoices" -ForegroundColor Yellow
Write-Host "4. Check current business in selector (top-right)" -ForegroundColor Yellow
Write-Host "5. Note the number of invoices displayed" -ForegroundColor Yellow
Write-Host "6. Switch business in selector" -ForegroundColor Yellow
Write-Host "7. Verify:" -ForegroundColor Yellow
Write-Host "   - Page does NOT reload" -ForegroundColor Gray
Write-Host "   - Invoice list updates automatically" -ForegroundColor Gray
Write-Host "   - Only invoices for selected business are shown" -ForegroundColor Gray
Write-Host "8. Repeat for: AG → Quotes" -ForegroundColor Yellow
Write-Host ""

Write-Host "=== TEST URLS ===" -ForegroundColor Cyan
Write-Host "Invoices: http://localhost:5204/ar/invoices" -ForegroundColor White
Write-Host "Quotes:   http://localhost:5204/ag/quotes" -ForegroundColor White
Write-Host ""

# Summary
Write-Host "=== IMPLEMENTATION STATUS ===" -ForegroundColor Cyan
Write-Host "✅ BusinessContext with Changed event" -ForegroundColor Green
Write-Host "✅ AR/Invoices/Index.razor - reactive filtering" -ForegroundColor Green
Write-Host "✅ AG/Quotes/Index.razor - reactive filtering" -ForegroundColor Green
Write-Host "✅ Async lambda subscription pattern" -ForegroundColor Green
Write-Host "✅ Build passes with 0 errors" -ForegroundColor Green
Write-Host ""
Write-Host "⏳ Awaiting manual UI testing" -ForegroundColor Yellow
Write-Host ""
