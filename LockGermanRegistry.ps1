# Lock German keyboard in registry to prevent duplicates

Write-Host "Locking German keyboard configuration in registry..." -ForegroundColor Cyan

$regPath = "HKCU:\Keyboard Layout\Preload"

try {
    # Check current preload
    $preload = Get-Item -Path $regPath -ErrorAction Stop
    Write-Host "`nCurrent keyboard preload:" -ForegroundColor Yellow
    $preload.Property | ForEach-Object { Write-Host "  $($_): $(($preload).GetValue($_))" }
    
    # Set preload to Russian + German Austria only
    Set-ItemProperty -Path $regPath -Name "1" -Value "00000419" -Force
    Set-ItemProperty -Path $regPath -Name "2" -Value "00000c07" -Force
    
    # Remove any extra entries (3+)
    $preload.Property | Where-Object { $_ -match '^\d+$' -and [int]$_ -gt 2 } | ForEach-Object {
        Remove-ItemProperty -Path $regPath -Name $_ -Force
        Write-Host "Removed extra keyboard: $($_)" -ForegroundColor Red
    }
    
    Write-Host "`nNew keyboard preload:" -ForegroundColor Green
    $newPreload = Get-Item -Path $regPath
    $newPreload.Property | ForEach-Object { 
        Write-Host "  $($_): $(($newPreload).GetValue($_))"
    }
    
    Write-Host "`nRegistry locked successfully!" -ForegroundColor Green
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
