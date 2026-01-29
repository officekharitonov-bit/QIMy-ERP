#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;

var dbPath = @"c:\Projects\QIMy\src\QIMy.Web\QImyDb.db";
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("--- Current Invoices in System ---\n");

var connection = new SqliteConnection(connectionString);
connection.Open();

var command = connection.CreateCommand();
command.CommandText = @"
    SELECT
        i.Id,
        i.InvoiceNumber,
        i.InvoiceDate,
        i.ClientId,
        c.CompanyName,
        i.TotalAmount,
        i.Status
    FROM Invoices i
    LEFT JOIN Clients c ON i.ClientId = c.Id
    ORDER BY i.InvoiceDate DESC;
";

var reader = command.ExecuteReader();
Console.WriteLine("ID | Number | Date | Client | Amount | Status");
Console.WriteLine(new string('-', 80));

while (reader.Read())
{
    Console.WriteLine($"{reader[0]} | {reader[1]} | {reader[2]?.ToString().Substring(0, 10)} | {reader[4] ?? "NULL"} | {reader[5]} | {reader[6]}");
}

connection.Close();

Console.WriteLine("\n--- Client Information ---\n");
connection.Open();

command = connection.CreateCommand();
command.CommandText = @"
    SELECT
        Id,
        CompanyName,
        Country,
        VatNumber
    FROM Clients
    ORDER BY CompanyName;
";

reader = command.ExecuteReader();
Console.WriteLine("ID | CompanyName | Country | VatNumber");
Console.WriteLine(new string('-', 80));

while (reader.Read())
{
    Console.WriteLine($"{reader[0]} | {reader[1]} | {reader[2]} | {reader[3]}");
}

connection.Close();
