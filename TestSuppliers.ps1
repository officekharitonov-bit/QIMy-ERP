# Test Suppliers Module - Full CRUD + Duplicate Detection
$baseUrl = "http://localhost:5215"
$businessId = 1

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ТЕСТИРОВАНИЕ МОДУЛЯ SUPPLIERS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Helper function to display results
function Show-Result {
    param($title, $response, $statusCode)
    Write-Host "`n--- $title ---" -ForegroundColor Yellow
    Write-Host "Status: $statusCode" -ForegroundColor $(if($statusCode -lt 400) { "Green" } else { "Red" })
    if ($response) {
        $response | ConvertTo-Json -Depth 5
    }
}

# Test 1: Get all suppliers (должно быть пусто)
Write-Host "`n[TEST 1] GET /api/suppliers - Получить все поставщики" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers" -Method Get
    Show-Result "Список поставщиков" $response 200
} catch {
    Write-Host "Ошибка: $_" -ForegroundColor Red
}

# Test 2: Create new supplier
Write-Host "`n[TEST 2] POST /api/suppliers - Создать поставщика" -ForegroundColor Cyan
$newSupplier = @{
    businessId = $businessId
    companyName = "ООО Тестовый Поставщик"
    contactPerson = "Иван Иванов"
    email = "test@supplier.com"
    phone = "+43 123 456789"
    address = "Тестовая улица 123"
    city = "Вена"
    postalCode = "1010"
    country = "Австрия"
    taxNumber = "123456789"
    vatNumber = "ATU12345678"
    bankAccount = "AT123456789012345678"
    ignoreDuplicateWarning = $false
    doubleConfirmed = $false
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers" -Method Post -Body $newSupplier -ContentType "application/json"
    $supplierId = $response.id
    Show-Result "Создан поставщик" $response 201
    Write-Host "✅ Supplier ID: $supplierId" -ForegroundColor Green
} catch {
    $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "Ошибка: $($errorDetails | ConvertTo-Json -Depth 3)" -ForegroundColor Red
}

# Test 3: Get supplier by ID
if ($supplierId) {
    Write-Host "`n[TEST 3] GET /api/suppliers/$supplierId - Получить по ID" -ForegroundColor Cyan
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers/$supplierId" -Method Get
        Show-Result "Поставщик по ID" $response 200
    } catch {
        Write-Host "Ошибка: $_" -ForegroundColor Red
    }
}

# Test 4: Try to create DUPLICATE (should fail with warning)
Write-Host "`n[TEST 4] POST /api/suppliers - ДУБЛИКАТ (должен выдать предупреждение)" -ForegroundColor Cyan
$duplicateSupplier = @{
    businessId = $businessId
    companyName = "ООО Тестовый Поставщик"  # Same name
    contactPerson = "Петр Петров"
    email = "duplicate@supplier.com"
    phone = "+43 987 654321"
    vatNumber = "ATU98765432"
    ignoreDuplicateWarning = $false
    doubleConfirmed = $false
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers" -Method Post -Body $duplicateSupplier -ContentType "application/json"
    Write-Host "❌ ОШИБКА: Дубликат должен был быть заблокирован!" -ForegroundColor Red
} catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "`n✅ Ожидаемое предупреждение:" -ForegroundColor Green
    Write-Host $errorResponse.error -ForegroundColor Yellow
}

# Test 5: Create duplicate with IgnoreDuplicateWarning (should ask for DoubleConfirmed)
Write-Host "`n[TEST 5] POST - ДУБЛИКАТ с IgnoreDuplicateWarning=true" -ForegroundColor Cyan
$duplicateSupplier2 = @{
    businessId = $businessId
    companyName = "ООО Тестовый Поставщик"
    contactPerson = "Петр Петров"
    email = "duplicate@supplier.com"
    ignoreDuplicateWarning = $true
    doubleConfirmed = $false
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers" -Method Post -Body $duplicateSupplier2 -ContentType "application/json"
    Write-Host "❌ ОШИБКА: Должно требовать DoubleConfirmed!" -ForegroundColor Red
} catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "`n✅ Ожидаемое второе предупреждение:" -ForegroundColor Green
    Write-Host $errorResponse.error -ForegroundColor Yellow
}

# Test 6: Create duplicate with BOTH flags (should succeed)
Write-Host "`n[TEST 6] POST - ДУБЛИКАТ с двойным подтверждением" -ForegroundColor Cyan
$duplicateSupplier3 = @{
    businessId = $businessId
    companyName = "ООО Тестовый Поставщик Копия"
    contactPerson = "Петр Петров"
    email = "duplicate2@supplier.com"
    phone = "+43 111 222333"
    vatNumber = "ATU11122233"
    ignoreDuplicateWarning = $true
    doubleConfirmed = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers" -Method Post -Body $duplicateSupplier3 -ContentType "application/json"
    $duplicateId = $response.id
    Show-Result "Дубликат создан с подтверждением" $response 201
    Write-Host "✅ Duplicate Supplier ID: $duplicateId" -ForegroundColor Green
} catch {
    Write-Host "Ошибка: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# Test 7: Update supplier
if ($supplierId) {
    Write-Host "`n[TEST 7] PUT /api/suppliers/$supplierId - Обновить поставщика" -ForegroundColor Cyan
    $updateSupplier = @{
        id = $supplierId
        businessId = $businessId
        companyName = "ООО Тестовый Поставщик ОБНОВЛЕН"
        contactPerson = "Иван Иванов UPDATED"
        email = "updated@supplier.com"
        phone = "+43 999 888777"
        address = "Новая улица 456"
        city = "Зальцбург"
        postalCode = "5020"
        country = "Австрия"
        taxNumber = "999888777"
        vatNumber = "ATU99988877"
        bankAccount = "AT999888777666555444"
        ignoreDuplicateWarning = $false
        doubleConfirmed = $false
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers/$supplierId" -Method Put -Body $updateSupplier -ContentType "application/json"
        Show-Result "Поставщик обновлен" $response 200
    } catch {
        Write-Host "Ошибка: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# Test 8: Search suppliers
Write-Host "`n[TEST 8] GET /api/suppliers?searchTerm=тестовый - Поиск" -ForegroundColor Cyan
try {
    $encodedSearch = [System.Web.HttpUtility]::UrlEncode("тестовый")
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers?searchTerm=$encodedSearch" -Method Get
    Show-Result "Результаты поиска" $response 200
    Write-Host "Найдено: $($response.Count) поставщиков" -ForegroundColor Green
} catch {
    Write-Host "Ошибка: $_" -ForegroundColor Red
}

# Test 9: Create CSV for import test
Write-Host "`n[TEST 9] Создание CSV для импорта" -ForegroundColor Cyan
$csvContent = @"
CompanyName;ContactPerson;Email;Phone;VatNumber
ООО CSV Поставщик 1;Контакт 1;csv1@test.com;+43 111 111111;ATU11111111
ООО CSV Поставщик 2;Контакт 2;csv2@test.com;+43 222 222222;ATU22222222
ООО CSV Поставщик 3;Контакт 3;csv3@test.com;+43 333 333333;ATU33333333
"@
$csvPath = "C:\Projects\QIMy\test_suppliers.csv"
$csvContent | Out-File -FilePath $csvPath -Encoding UTF8
Write-Host "✅ CSV файл создан: $csvPath" -ForegroundColor Green

# Test 10: Import CSV
Write-Host "`n[TEST 10] POST /api/suppliers/import - Импорт из CSV" -ForegroundColor Cyan
try {
    $boundary = [System.Guid]::NewGuid().ToString()
    $fileBin = [System.IO.File]::ReadAllBytes($csvPath)
    $enc = [System.Text.Encoding]::GetEncoding("UTF-8")
    
    $bodyLines = @(
        "--$boundary",
        "Content-Disposition: form-data; name=`"file`"; filename=`"test_suppliers.csv`"",
        "Content-Type: text/csv",
        "",
        $enc.GetString($fileBin),
        "--$boundary",
        "Content-Disposition: form-data; name=`"businessId`"",
        "",
        "$businessId",
        "--$boundary--"
    ) -join "`r`n"
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers/import" -Method Post -ContentType "multipart/form-data; boundary=$boundary" -Body $bodyLines
    Show-Result "Результат импорта" $response 200
    Write-Host "✅ Импортировано: $($response.successCount) из $($response.totalRows)" -ForegroundColor Green
} catch {
    Write-Host "Ошибка импорта: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# Test 11: Export CSV
Write-Host "`n[TEST 11] GET /api/suppliers/export - Экспорт в CSV" -ForegroundColor Cyan
try {
    $exportPath = "C:\Projects\QIMy\exported_suppliers.csv"
    Invoke-WebRequest -Uri "$baseUrl/api/suppliers/export?businessId=$businessId" -OutFile $exportPath
    Write-Host "✅ CSV экспортирован: $exportPath" -ForegroundColor Green
    Write-Host "Первые 5 строк:" -ForegroundColor Yellow
    Get-Content $exportPath -First 5
} catch {
    Write-Host "Ошибка экспорта: $_" -ForegroundColor Red
}

# Test 12: Get all suppliers again (should have multiple now)
Write-Host "`n[TEST 12] GET /api/suppliers - Финальный список" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers?businessId=$businessId" -Method Get
    Write-Host "✅ Всего поставщиков: $($response.Count)" -ForegroundColor Green
    $response | Format-Table Id, CompanyName, Email, VatNumber -AutoSize
} catch {
    Write-Host "Ошибка: $_" -ForegroundColor Red
}

# Test 13: Bulk delete (cleanup)
Write-Host "`n[TEST 13] POST /api/suppliers/bulk-delete - Массовое удаление" -ForegroundColor Cyan
try {
    $allSuppliers = Invoke-RestMethod -Uri "$baseUrl/api/suppliers?businessId=$businessId" -Method Get
    $idsToDelete = $allSuppliers | Select-Object -First 2 -ExpandProperty id
    
    if ($idsToDelete.Count -gt 0) {
        $deleteBody = $idsToDelete | ConvertTo-Json
        $response = Invoke-RestMethod -Uri "$baseUrl/api/suppliers/bulk-delete" -Method Post -Body $deleteBody -ContentType "application/json"
        Show-Result "Результат массового удаления" $response 200
        Write-Host "✅ Удалено: $($response.successCount) из $($response.total)" -ForegroundColor Green
    } else {
        Write-Host "Нет поставщиков для удаления" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Ошибка: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# Test 14: Delete single supplier
if ($supplierId) {
    Write-Host "`n[TEST 14] DELETE /api/suppliers/$supplierId - Удалить поставщика" -ForegroundColor Cyan
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/suppliers/$supplierId" -Method Delete
        Write-Host "✅ Поставщик удален" -ForegroundColor Green
    } catch {
        Write-Host "Ошибка: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# Final summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ТЕСТИРОВАНИЕ ЗАВЕРШЕНО" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Проверьте результаты выше для:" -ForegroundColor Yellow
Write-Host "✓ CRUD операции (Create, Read, Update, Delete)" -ForegroundColor Green
Write-Host "✓ Проверка дубликатов (3-этапная)" -ForegroundColor Green
Write-Host "✓ CSV импорт/экспорт" -ForegroundColor Green
Write-Host "✓ Поиск и фильтрация" -ForegroundColor Green
Write-Host "✓ Массовое удаление" -ForegroundColor Green
