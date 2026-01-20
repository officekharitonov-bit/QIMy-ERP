$ErrorActionPref = 'Stop'

# Путь к базе
$dbPath = "C:\Projects\QIMy\src\QIMy.Web\QImyDb.db"

# Пароль для хеширования: Admin123!
# Hash сгенерирован с помощью ASP.NET Core Identity PasswordHasher
$passwordHash = "AQAAAAIAAYagAAAAEOZUiZFIf8eaMl1g8VDa4nJOeD0KVh1m9vJyFZCvhZiVdSLPVkXxqMI4KLSMHDMCpg=="

# SQL запрос
$sql = @"
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, 
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, 
    LockoutEnabled, AccessFailedCount, FirstName, LastName, BusinessId, 
    Role, IsActive, CreatedAt, UpdatedAt
) VALUES (
    'admin-001', 
    'office@kharitonov.at', 
    'OFFICE@KHARITONOV.AT', 
    'office@kharitonov.at', 
    'OFFICE@KHARITONOV.AT', 
    1, 
    '$passwordHash', 
    '$(New-Guid)', 
    '$(New-Guid)', 
    NULL, 
    0, 
    0, 
    NULL, 
    1, 
    0, 
    'Egor', 
    'Kharitonov', 
    NULL, 
    'Admin', 
    1, 
    '$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")', 
    NULL
);
"@

# Используем System.Data.SQLite
Add-Type -Path "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Data.SQLite\v4.0_1.0.118.0__db937bc2d44ff139\System.Data.SQLite.dll" -ErrorAction SilentlyContinue

try {
    $conn = New-Object System.Data.SQLite.SQLiteConnection
    $conn.ConnectionString = "Data Source=$dbPath"
    $conn.Open()
    
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $result = $cmd.ExecuteNonQuery()
    
    Write-Host "✅ Пользователь успешно создан!" -ForegroundColor Green
    Write-Host "Email: office@kharitonov.at" -ForegroundColor Cyan
    Write-Host "Password: Admin123!" -ForegroundColor Cyan
    
    $conn.Close()
}
catch {
    Write-Host "❌ Ошибка: $_" -ForegroundColor Red
}
