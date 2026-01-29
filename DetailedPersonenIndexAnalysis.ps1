# Detailed analysis of Personen Index.xlsx

$filePath = "C:\Projects\QIMy\tabellen\Personen Index.xlsx"
$reportFile = "C:\Projects\QIMy\PersonenIndex_Analysis.txt"

$report = @()
$report += "=" * 90
$report += "PERSONEN INDEX.XLSX - DETAILED STRUCTURE ANALYSIS"
$report += "=" * 90
$report += ""

try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $workbook = $excel.Workbooks.Open($filePath)

    $sheetCount = $workbook.Sheets.Count

    foreach ($i in 1..$sheetCount) {
        $sheet = $workbook.Sheets.Item($i)
        $usedRange = $sheet.UsedRange
        $lastRow = $usedRange.Rows.Count
        $lastCol = $usedRange.Columns.Count

        $report += "SHEET $($i): $($sheet.Name)"
        $report += "-" * 90
        $report += "Dimensions: $lastRow rows x $lastCol columns"
        $report += ""

        # Header row
        $headers = @()
        for ($col = 1; $col -le $lastCol; $col++) {
            $cell = $sheet.Cells.Item(1, $col)
            $val = $cell.Value2
            if ($val) { $headers += $val.ToString().Trim() } else { $headers += "[EMPTY]" }
        }

        $report += "COLUMNS:"
        for ($col = 0; $col -lt $headers.Count; $col++) {
            $report += "  Col $($col+1): $($headers[$col])"
        }
        $report += ""

        # Sample data rows
        if ($lastRow -gt 1) {
            $report += "SAMPLE DATA (first 5 rows):"
            for ($row = 2; $row -le [Math]::Min(6, $lastRow); $row++) {
                $rowData = @()
                for ($col = 1; $col -le $lastCol; $col++) {
                    $cell = $sheet.Cells.Item($row, $col)
                    $val = $cell.Value2
                    if ($val) { $rowData += $val.ToString() } else { $rowData += "[EMPTY]" }
                }
                $report += "  Row $row : $($rowData -join ' | ')"
            }
        }

        $report += ""
        $report += ""
    }

    # Summary
    $report += "=" * 90
    $report += "SUMMARY"
    $report += "=" * 90
    $report += ""
    $report += "Sheet 1: Personen Index"
    $report += "  Purpose: Main person/client data sheet (appears to be empty or formula-based)"
    $report += "  Used by: Contains formulas referencing other sheets"
    $report += ""
    $report += "Sheet 2: EU-RATE"
    $report += "  Purpose: EU VAT rates and country reference data"
    $report += "  Columns: Country name, ISO code, VAT rate, German name, threshold values, currency, account number"
    $report += "  Key field: Code (ISO 2-letter country code)"
    $report += ""
    $report += "Sheet 3: CODE_INDEX"
    $report += "  Purpose: Accounting codes and account descriptions"
    $report += "  Columns: Account number, Description (German), Description (Hungarian), Account type, Code"
    $report += "  Key field: Kto-Nr (Account number)"
    $report += ""
    $report += "Sheet 4: Steuercodes"
    $report += "  Purpose: Tax codes and automatic posting rules"
    $report += "  Columns: Account number, Description, Account type, Code, Tax codes, Automation rules"
    $report += "  Key field: Kto-Nr (Account number)"
    $report += ""
    $report += "Sheet 5: Sorted list"
    $report += "  Purpose: Reference list of account numbers with country codes"
    $report += "  Columns: Account number, Country code (Freifeld 01), Description"
    $report += "  Key field: Kto-Nr (Account number)"
    $report += ""
    $report += "Sheet 6: Laender (Countries)"
    $report += "  Purpose: Country master data with numeric country codes"
    $report += "  Columns: Country name (German), Account range?, English name, Country number"
    $report += "  Key field: Country number"
    $report += ""

    $workbook.Close($false)
    $excel.Quit()

    # Write report
    $report | Out-File -FilePath $reportFile -Encoding UTF8 -Force

    Write-Host "Analysis complete! Report saved to:" -ForegroundColor Green
    Write-Host $reportFile -ForegroundColor Cyan
    Write-Host ""
    $report | Out-Host

}
catch {
    Write-Host "ERROR: $($_)" -ForegroundColor Red
    if ($excel) { $excel.Quit() }
    exit 1
}
