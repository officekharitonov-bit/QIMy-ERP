#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;

var dbPath = @"c:\Projects\QIMy\src\QIMy.Web\QImyDb.db";
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("=== Creating Sample Invoices ===\n");

var connection = new SqliteConnection(connectionString);
connection.Open();

// First, get business and clients
var command = connection.CreateCommand();
command.CommandText = "SELECT Id FROM Businesses LIMIT 1;";
var businessId = (long?)command.ExecuteScalar() ?? 1;
Console.WriteLine($"Using BusinessId: {businessId}");

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
    var invoiceNumber = $"2026{(i+1):D3}";
    var invoiceDate = new DateTime(2026, 1, 15 + i);
    var dueDate = new DateTime(2026, 2, 15 + i);
    var amount = (i + 1) * 60m;
    var tax = (i + 1) * 12m;
    var gross = (i + 1) * 72m;
    
    // Determine invoice type based on country
    var invoiceType = country == "Austria" ? "Domestic" : "IntraEU";
    
    command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Invoices (
            InvoiceNumber, InvoiceDate, DueDate, ClientId, BusinessId,
            Currency, Status, InvoiceType, TotalAmount, TaxAmount, GrossAmount,
            CreatedAt, UpdatedAt, Notes
        ) VALUES (
            @number, @date, @due, @client, @business,
            'EUR', 'Draft', @type, @total, @tax, @gross,
            @created, @updated, @notes
        );
    ";
    
    command.Parameters.AddWithValue("@number", invoiceNumber);
    command.Parameters.AddWithValue("@date", invoiceDate.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("@due", dueDate.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("@client", clientId);
    command.Parameters.AddWithValue("@business", businessId);
    command.Parameters.AddWithValue("@type", invoiceType);
    command.Parameters.AddWithValue("@total", amount);
    command.Parameters.AddWithValue("@tax", tax);
    command.Parameters.AddWithValue("@gross", gross);
    command.Parameters.AddWithValue("@created", now.ToString("o"));
    command.Parameters.AddWithValue("@updated", now.ToString("o"));
    command.Parameters.AddWithValue("@notes", $"Sample invoice for {clientName}");
    
    command.ExecuteNonQuery();
    Console.WriteLine($"✓ Invoice {invoiceNumber} created for {clientName} ({country})");
}

connection.Close();

Console.WriteLine($"\n✅ Successfully created {clients.Count} invoices");
