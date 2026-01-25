#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;

var dbPath = @"c:\Projects\QIMy\src\QIMy.Web\QImyDb.db";
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("=== Creating Sample Invoices with Different Tax Cases ===\n");

var connection = new SqliteConnection(connectionString);
connection.Open();

// Get business and currency
var command = connection.CreateCommand();
command.CommandText = "SELECT Id FROM Businesses LIMIT 1;";
var businessId = (long?)command.ExecuteScalar() ?? 1;

command.CommandText = "SELECT Id FROM Currencies WHERE Code = 'EUR' LIMIT 1;";
var currencyId = (long?)command.ExecuteScalar();
if (currencyId == null)
{
    command.CommandText = @"
        INSERT INTO Currencies (Code, Symbol, Name, IsActive, CreatedAt, UpdatedAt, IsDeleted)
        VALUES ('EUR', '€', 'Euro', 1, datetime('now'), datetime('now'), 0);
    ";
    command.ExecuteNonQuery();
    command.CommandText = "SELECT last_insert_rowid();";
    currencyId = (long)command.ExecuteScalar();
}

Console.WriteLine($"BusinessId: {businessId}, CurrencyId: {currencyId}\n");

// Get clients
command.CommandText = @"
    SELECT Id, CompanyName, Country FROM Clients 
    WHERE IsDeleted = 0
    ORDER BY CompanyName;
";

var reader = command.ExecuteReader();
var clients = new List<(long Id, string Name, string Country)>();
while (reader.Read())
{
    clients.Add(((long)reader[0], (string)reader[1], (string)reader[2]));
}
reader.Close();

Console.WriteLine($"Found {clients.Count} clients\n");

// Invoice types: 0=Domestic, 1=Export, 2=IntraEUSale, 3=ReverseCharge, 4=SmallBusinessExemption
var invoices = new List<(string Number, long ClientId, string ClientName, string InvoiceType, decimal SubTotal, decimal Tax, string Description)>
{
    // Case 1: Domestic (Inland) - 20% VAT
    ("2026001", clients[0].Id, clients[0].Name, "Domestic", 100m, 20m, 
     "INLAND: Domestic supply Austria → 20% VAT"),
    
    // Case 2: Export (Exportrechnung) - 0% VAT
    ("2026002", clients[1].Id, clients[1].Name, "Export", 100m, 0m,
     "EXPORT: Tax-free export to Slovakia → 0% VAT"),
    
    // Case 3: Intra-EU Sale (Innergemeinschaftliche Lieferung) - 0% VAT in AT
    ("2026003", clients[1].Id, clients[1].Name, "IntraEUSale", 120m, 0m,
     "INTRA-EU: Intra-EU supply to Slovakia → 0% VAT (reverse charge)"),
    
    // Case 4: Reverse Charge - 0% VAT
    ("2026004", clients[2].Id, clients[2].Name, "ReverseCharge", 150m, 0m,
     "REVERSE CHARGE: VAT liability on customer → 0% VAT"),
    
    // Case 5: Small Business (Kleinunternehmer) - no VAT
    ("2026005", clients[2].Id, clients[2].Name, "SmallBusinessExemption", 80m, 0m,
     "KLEINUNTERNEHMER: Small business exemption → 0% VAT"),
};

var now = DateTime.UtcNow;
foreach (var (number, clientId, clientName, invType, subTotal, tax, description) in invoices)
{
    var totalAmount = subTotal + tax;
    var invoiceDate = new DateTime(2026, 1, 20 + (number[^1] - '0'));
    var dueDate = invoiceDate.AddDays(30);
    
    // Map string to enum value (0=Domestic, 1=Export, 2=IntraEUSale, 3=ReverseCharge, 4=SmallBusinessExemption)
    var typeValue = invType switch
    {
        "Domestic" => 0,
        "Export" => 1,
        "IntraEUSale" => 2,
        "ReverseCharge" => 3,
        "SmallBusinessExemption" => 4,
        _ => 0
    };
    
    // Determine tax flags
    var isReverseCharge = invType == "ReverseCharge" ? 1 : 0;
    var isSmallBusiness = invType == "SmallBusinessExemption" ? 1 : 0;
    var isTaxFree = invType == "Export" ? 1 : 0;
    var isIntraEU = invType == "IntraEUSale" ? 1 : 0;
    
    command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Invoices (
            InvoiceNumber, InvoiceDate, DueDate, ClientId, BusinessId,
            CurrencyId, Status, SubTotal, TaxAmount, TotalAmount, PaidAmount,
            CreatedAt, UpdatedAt, Notes, IsDeleted,
            InvoiceType, IsReverseCharge, IsSmallBusinessExemption,
            IsTaxFreeExport, IsIntraEUSale
        ) VALUES (
            @number, @date, @due, @client, @business,
            @currency, 'Draft', @subTotal, @tax, @total, 0,
            @created, @updated, @notes, 0,
            @type, @revCharge, @smallBiz, @taxFree, @intraEu
        );
    ";
    
    command.Parameters.AddWithValue("@number", number);
    command.Parameters.AddWithValue("@date", invoiceDate.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("@due", dueDate.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("@client", clientId);
    command.Parameters.AddWithValue("@business", businessId);
    command.Parameters.AddWithValue("@currency", currencyId);
    command.Parameters.AddWithValue("@subTotal", subTotal);
    command.Parameters.AddWithValue("@tax", tax);
    command.Parameters.AddWithValue("@total", totalAmount);
    command.Parameters.AddWithValue("@created", now.ToString("o"));
    command.Parameters.AddWithValue("@updated", now.ToString("o"));
    command.Parameters.AddWithValue("@notes", description);
    command.Parameters.AddWithValue("@type", typeValue);
    command.Parameters.AddWithValue("@revCharge", isReverseCharge);
    command.Parameters.AddWithValue("@smallBiz", isSmallBusiness);
    command.Parameters.AddWithValue("@taxFree", isTaxFree);
    command.Parameters.AddWithValue("@intraEu", isIntraEU);
    
    command.ExecuteNonQuery();
    
    Console.WriteLine($"✓ {number} | {invType.PadRight(20)} | €{totalAmount:F2} | {clientName} | {description}");
}

connection.Close();

Console.WriteLine($"\n✅ Successfully created {invoices.Count} invoices with different tax cases!");
Console.WriteLine("\nInvoice Types:");
Console.WriteLine("  0 = Domestic (INLAND) - standard 20% VAT");
Console.WriteLine("  1 = Export (Exportrechnung) - 0% VAT");
Console.WriteLine("  2 = Intra-EU Sale - 0% VAT (customer reports)");
Console.WriteLine("  3 = Reverse Charge - 0% VAT (liability on customer)");
Console.WriteLine("  4 = Small Business (Kleinunternehmer) - 0% VAT (exemption)");
