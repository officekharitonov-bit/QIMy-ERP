#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.SqlClient, 5.1.0"

using Microsoft.Data.SqlClient;

var connectionString = "Server=qimy-sql-server.database.windows.net;Database=QImyDb;User Id=qimyadmin;Password=h970334054CRgd1!;TrustServerCertificate=True;";

var connection = new SqlConnection(connectionString);
connection.Open();

Console.WriteLine("=== BUSINESSES ===");
using (var cmd = new SqlCommand("SELECT Id, Name FROM Businesses ORDER BY Id", connection))
using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
    {
        Console.WriteLine($"ID: {reader["Id"]}, Name: {reader["Name"]}");
    }
}

Console.WriteLine("\n=== INVOICES BY BUSINESS ===");
using (var cmd = new SqlCommand("SELECT BusinessId, COUNT(*) as Count FROM Invoices GROUP BY BusinessId ORDER BY BusinessId", connection))
using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
    {
        var businessId = reader["BusinessId"] == DBNull.Value ? "NULL" : reader["BusinessId"].ToString();
        Console.WriteLine($"BusinessId: {businessId}, Count: {reader["Count"]}");
    }
}

Console.WriteLine("\n=== QUOTES BY BUSINESS ===");
using (var cmd = new SqlCommand("SELECT BusinessId, COUNT(*) as Count FROM Quotes GROUP BY BusinessId ORDER BY BusinessId", connection))
using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
    {
        var businessId = reader["BusinessId"] == DBNull.Value ? "NULL" : reader["BusinessId"].ToString();
        Console.WriteLine($"BusinessId: {businessId}, Count: {reader["Count"]}");
    }
}
