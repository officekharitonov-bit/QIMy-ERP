# Read Personen Index structure
$excelPath = "c:\Projects\QIMy\tabellen\Personen index.xlsx"

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false

try {
    $wb = $excel.Workbooks.Open($excelPath)

    # EU-RATE sheet (Sheet 2)
    Write-Host "=== EU-RATE SHEET (Sheet 2) ===" -ForegroundColor Cyan
    $ws = $wb.Worksheets.Item(2)
    Write-Host "Columns:"
    for ($i = 1; $i -le 13; $i++) {
        $header = $ws.Cells.Item(1, $i).Text
        Write-Host "  Col $i - $header"
    }

    Write-Host ""
    Write-Host "Sample rows (2-5):"
    for ($row = 2; $row -le 5; $row++) {
        $line = ""
        for ($col = 1; $col -le 13; $col++) {
            $value = $ws.Cells.Item($row, $col).Text
            $line += "$value | "
        }
        Write-Host "  Row $row - $line"
    }

    # Länder sheet (Sheet 6)
    Write-Host ""
    Write-Host ""
    Write-Host "=== LAENDER SHEET (Sheet 6) ===" -ForegroundColor Cyan
    $ws6 = $wb.Worksheets.Item(6)
    $maxCol = 10
    Write-Host "Columns:"
    for ($i = 1; $i -le $maxCol; $i++) {
        $header = $ws6.Cells.Item(1, $i).Text
        if ($header) {
            Write-Host "  Col $i - $header"
        }
    }

    Write-Host ""
    Write-Host "Sample rows (2-5):"
    for ($row = 2; $row -le 5; $row++) {
        $line = ""
        for ($col = 1; $col -le $maxCol; $col++) {
            $value = $ws6.Cells.Item($row, $col).Text
            if ($value) {
                $line += "$value ; "
            }
        }
        if ($line) {
            Write-Host "  Row $row - $line"
        }
    }

    $wb.Close($false)
}
finally {
    $excel.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
}
