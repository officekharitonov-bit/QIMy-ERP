# Generate AI Context from Session Logs
# Usage: .\Generate-AIContext.ps1

$outputFile = "AI_CONTEXT_GENERATED.md"
$sessionLogsPath = "."

Write-Host "🤖 Generating AI Context from session logs..." -ForegroundColor Cyan

$context = @"
# 🤖 AI Context - Auto-Generated
**Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Source:** Session logs (session-logs/)

---

## 📋 Recent Sessions Summary

"@

# Get last 5 session logs
$sessionLogs = Get-ChildItem -Path $sessionLogsPath -Filter "SESSION_LOG_*.md" -File |
Sort-Object LastWriteTime -Descending |
Select-Object -First 5

foreach ($log in $sessionLogs) {
    $date = $log.BaseName -replace "SESSION_LOG_", "" -replace "_.*", ""
    $formattedDate = "$($date.Substring(0,4))-$($date.Substring(4,2))-$($date.Substring(6,2))"
    $topic = $log.BaseName -replace "SESSION_LOG_\d{8}_", "" -replace "_", " "

    $context += @"

### $formattedDate - $topic
**File:** [$($log.Name)]($($log.Name))

"@

    # Extract key points from log (first 20 lines)
    $content = Get-Content $log.FullName -TotalCount 50 | Where-Object { $_ -match "^(##|###|\*\*|✅)" }

    if ($content) {
        $context += "**Key Points:**`n"
        $content | Select-Object -First 5 | ForEach-Object {
            $context += "$_`n"
        }
    }

    $context += "`n"
}

# Add quick stats
$allLogs = Get-ChildItem -Path $sessionLogsPath -Filter "SESSION_LOG_*.md" -File
$context += @"

---

## 📊 Statistics
- Total Sessions: $($allLogs.Count)
- Date Range: $(($allLogs | Sort-Object LastWriteTime | Select-Object -First 1).LastWriteTime.ToString("yyyy-MM-dd")) to $(($allLogs | Sort-Object LastWriteTime | Select-Object -Last 1).LastWriteTime.ToString("yyyy-MM-dd"))
- Latest Update: $(($allLogs | Sort-Object LastWriteTime | Select-Object -Last 1).LastWriteTime.ToString("yyyy-MM-dd HH:mm"))

---

## 🔍 How to Use This Context

**For AI:**
1. Read this file at the start of each session
2. For detailed info, read specific session log from above list
3. Always check `AI_CONTEXT.md` for current project status

**For Humans:**
1. Quick overview of recent work
2. Links to detailed session logs
3. Project timeline at a glance

---

*This file is auto-generated. Do not edit manually.*
*Run ``.\Generate-AIContext.ps1`` to regenerate.*
"@

# Save to file
$context | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host "✅ Context generated: $outputFile" -ForegroundColor Green
Write-Host "📄 Sessions included: $($sessionLogs.Count)" -ForegroundColor Yellow
Write-Host "`n📖 Content preview:" -ForegroundColor Cyan
Get-Content $outputFile -TotalCount 30

Write-Host "`n💡 Tip: Add this file to your AI prompt for automatic context!" -ForegroundColor Magenta
