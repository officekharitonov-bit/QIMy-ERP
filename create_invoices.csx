#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;

var dbPath = @"c:\Projects\QIMy\src\QIMy.Web\QImyDb.db";
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("=== Creating Sample Invoices ===\n");

var connection = new SqliteConnection(connectionString);
connection.Open();

// First, get business
var command = connection.CreateCommand();
command.CommandText = "SELECT Id FROM Businesses LIMIT 1;";
var businessId = (long?)command.ExecuteScalar() ?? 1;

// Get EUR currency
command.CommandText = "SELECT Id FROM Currencies WHERE Code = 'EUR' LIMIT 1;";
var currencyId = (long?)command.ExecuteScalar();
if (currencyId == null)
{
    Console.WriteLine("Warning: EUR currency not found, creating it");
    command.CommandText = @"
        INSERT INTO Currencies (Code, Symbol, Name, IsActive, CreatedAt, UpdatedAt)
        VALUES ('EUR', '€', 'Euro', 1, datetime('now'), datetime('now'));
    ";
    command.ExecuteNonQuery();
    command.CommandText = "SELECT last_insert_rowid();";
    currencyId = (long)command.ExecuteScalar();
}

Console.WriteLine($"Using BusinessId: {businessId}, CurrencyId: {currencyId}\n");

// Get clients
command.CommandText = @"
    SELECT Id, CompanyName, Country FROM Clients
    WHERE IsDeleted = 0
    ORDER BY CompanyName
    LIMIT 3;
";

var reader = command.ExecuteReader();
var clients = new List<(long Id, string Name, string Country)>();
while (reader.Read())
{
    clients.Add(((long)reader[0], (string)reader[1], (string)reader[2]));
}
reader.Close();

Console.WriteLine($"Found {clients.Count} clients\n");

// Create invoices
var now = DateTime.UtcNow;
for (int i = 0; i < clients.Count; i++)
{
    var (clientId, clientName, country) = clients[i];
    var invoiceNumber = $"2026{(i + 1):D3}";
    var invoiceDate = new DateTime(2026, 1, 15 + i);
    var dueDate = new DateTime(2026, 2, 15 + i);
    var subTotal = (decimal)((i + 1) * 60);
    var taxAmount = (decimal)((i + 1) * 12);
    var totalAmount = subTotal + taxAmount;

    command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Invoices (
            InvoiceNumber, InvoiceDate, DueDate, ClientId, BusinessId,
            CurrencyId, Status, SubTotal, TaxAmount, TotalAmount, PaidAmount,
            CreatedAt, UpdatedAt, Notes, IsDeleted
        ) VALUES (
            @number, @date, @due, @client, @business,
            @currency, 'Draft', @subTotal, @tax, @total, 0,
            @created, @updated, @notes, 0
        );
    ";

    command.Parameters.AddWithValue("@number", invoiceNumber);
    command.Parameters.AddWithValue("@date", invoiceDate.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("@due", dueDate.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("@client", clientId);
    command.Parameters.AddWithValue("@business", businessId);
    command.Parameters.AddWithValue("@currency", currencyId);
    command.Parameters.AddWithValue("@subTotal", subTotal);
    command.Parameters.AddWithValue("@tax", taxAmount);
    command.Parameters.AddWithValue("@total", totalAmount);
    command.Parameters.AddWithValue("@created", now.ToString("o"));
    command.Parameters.AddWithValue("@updated", now.ToString("o"));
    command.Parameters.AddWithValue("@notes", $"Sample invoice for {clientName}");

    command.ExecuteNonQuery();
    Console.WriteLine($"✓ Invoice {invoiceNumber} - €{totalAmount} for {clientName} ({country})");
}

connection.Close();

Console.WriteLine($"\n✅ Successfully created {clients.Count} invoices!");
