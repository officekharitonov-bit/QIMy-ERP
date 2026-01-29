# Scheduled task: ensure only one German keyboard persists
$taskName = "RemoveDuplicateGermanKeyboard"
$scriptPath = "C:\Projects\QIMy\RemoveDuplicateGerman.ps1"

# Check if task already exists
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if ($existingTask) {
    Write-Host "Task already exists" -ForegroundColor Green
    $existingTask | Select-Object TaskName, State
}
else {
    Write-Host "Creating scheduled task to prevent duplicate German keyboard..." -ForegroundColor Cyan

    $action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$scriptPath`""
    $trigger = New-ScheduledTaskTrigger -AtLogOn
    $principal = New-ScheduledTaskPrincipal -UserId "$env:USERNAME" -LogonType S4U -RunLevel Highest

    Register-ScheduledTask -TaskName $taskName `
        -Action $action `
        -Trigger $trigger `
        -Principal $principal `
        -Description "Automatically remove duplicate German keyboard layouts" `
        -Force

    Write-Host "Task created successfully!" -ForegroundColor Green
}
