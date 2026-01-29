#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.SqlServer, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

var cs = Environment.GetEnvironmentVariable("QIMy_Azure_ConnectionString") ??
         "Server=tcp:qimy-accounting.database.windows.net,1433;Database=QImyDB;User ID=qimyadmin;Password=P@ssw0rd2024!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

var options = new DbContextOptionsBuilder<DbContext>()
    .UseSqlServer(cs)
    .Options;

using var context = new DbContext(options);

Console.WriteLine("\n🔍 Checking client 230008 across all businesses:\n");

var sql = @"
SELECT
    c.Id,
    c.BusinessId,
    b.CompanyName as BusinessName,
    c.ClientCode,
    c.CompanyName as ClientName,
    c.VatNumber,
    c.CreatedAt
FROM Clients c
INNER JOIN Businesses b ON c.BusinessId = b.Id
WHERE c.ClientCode = 230008 AND c.IsDeleted = 0
ORDER BY c.BusinessId";

using var command = context.Database.GetDbConnection().CreateCommand();
command.CommandText = sql;
await context.Database.OpenConnectionAsync();

using var result = await command.ExecuteReaderAsync();
var count = 0;

while (await result.ReadAsync())
{
    count++;
    Console.WriteLine($"ClientId: {result["Id"]}");
    Console.WriteLine($"  BusinessId: {result["BusinessId"]} - {result["BusinessName"]}");
    Console.WriteLine($"  ClientCode: {result["ClientCode"]}");
    Console.WriteLine($"  ClientName: {result["ClientName"]}");
    Console.WriteLine($"  VAT: {result["VatNumber"]}");
    Console.WriteLine($"  Created: {result["CreatedAt"]}");
    Console.WriteLine();
}

Console.WriteLine($"✅ Total: {count} records with ClientCode=230008");

if (count > 1)
{
    Console.WriteLine("\n⚠️ ПРОБЛЕМА: Клиент с кодом 230008 существует в {0} бизнесах!", count);
    Console.WriteLine("   Это означает, что при импорте не был указан BusinessId или была ошибка фильтрации.");
}
else if (count == 1)
{
    Console.WriteLine("\n✅ ОК: Клиент существует только в одном бизнесе.");
}
else
{
    Console.WriteLine("\n❌ Клиент с кодом 230008 не найден в базе.");
}
