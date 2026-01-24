# Analyze Personen Index.xlsx structure

$filePath = "C:\Projects\QIMy\tabellen\Personen Index.xlsx"

Write-Host "Analyzing Personen Index.xlsx" -ForegroundColor Green
Write-Host "File: $filePath`n" -ForegroundColor Cyan

# Check if file exists
if (-not (Test-Path $filePath)) {
    Write-Host "ERROR: File not found!" -ForegroundColor Red
    exit 1
}

# Create Excel COM object
try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $workbook = $excel.Workbooks.Open($filePath)
    
    Write-Host "File opened successfully`n" -ForegroundColor Green
    
    # List all sheets
    Write-Host "SHEETS IN WORKBOOK:" -ForegroundColor Yellow
    Write-Host ("=" * 80)
    
    $sheetCount = $workbook.Sheets.Count
    Write-Host "Total sheets: $sheetCount`n"
    
    for ($i = 1; $i -le $sheetCount; $i++) {
        $sheet = $workbook.Sheets.Item($i)
        $usedRange = $sheet.UsedRange
        $lastRow = $usedRange.Rows.Count
        $lastCol = $usedRange.Columns.Count
        
        Write-Host "Sheet $i : $($sheet.Name)" -ForegroundColor Cyan
        Write-Host "  Dimensions: $lastRow rows x $lastCol columns"
        
        # Get header row
        if ($lastRow -gt 0) {
            $headers = @()
            for ($col = 1; $col -le [Math]::Min($lastCol, 15); $col++) {
                $cell = $sheet.Cells.Item(1, $col)
                $val = $cell.Value2
                if ($val) { $headers += $val.ToString() } else { $headers += "" }
            }
            Write-Host "  Headers: $($headers -join ' | ')"
        }
        
        # Show first 2 data rows
        if ($lastRow -gt 1) {
            Write-Host "  Sample data:"
            for ($row = 2; $row -le [Math]::Min(3, $lastRow); $row++) {
                $rowData = @()
                for ($col = 1; $col -le [Math]::Min($lastCol, 5); $col++) {
                    $cell = $sheet.Cells.Item($row, $col)
                    $val = $cell.Value2
                    if ($val) { $rowData += $val.ToString() } else { $rowData += "" }
                }
                Write-Host "    Row $row : $($rowData -join ' | ')"
            }
        }
        
        Write-Host ""
    }
    
    # Close workbook
    $workbook.Close($false)
    $excel.Quit()
    
    Write-Host "Analysis complete!" -ForegroundColor Green
    
} catch {
    Write-Host "ERROR: $($_)" -ForegroundColor Red
    if ($excel) { $excel.Quit() }
    exit 1
}
