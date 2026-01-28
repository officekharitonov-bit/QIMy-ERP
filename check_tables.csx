using System;
using System.Data.SqlClient;

var connString = @"Server=tcp:qimy-server.database.windows.net,1433;Initial Catalog=qimy-db;Persist Security Info=False;User ID=qimyadmin;Password=QIMy2024!Secure;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

using (var conn = new SqlConnection(connString)) {
    conn.Open();
    
    Console.WriteLine("=== ПРОВЕРКА КЛИЕНТОВ (200000-299999) ===");
    using (var cmd = new SqlCommand(@"
        SELECT COUNT(*) as Total, BusinessId 
        FROM Clients 
        WHERE ClientCode >= 200000 AND ClientCode < 300000
        GROUP BY BusinessId
        ORDER BY BusinessId", conn)) {
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"BusinessId={reader["BusinessId"]}: {reader["Total"]} клиентов");
            }
        }
    }
    
    Console.WriteLine("\n=== ПРОВЕРКА ПОСТАВЩИКОВ (300000-399999) ===");
    using (var cmd = new SqlCommand(@"
        SELECT COUNT(*) as Total, BusinessId 
        FROM Suppliers 
        WHERE SupplierCode >= 300000 AND SupplierCode < 400000
        GROUP BY BusinessId
        ORDER BY BusinessId", conn)) {
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"BusinessId={reader["BusinessId"]}: {reader["Total"]} поставщиков");
            }
        }
    }
    
    Console.WriteLine("\n=== ТОП 5 ЗАПИСЕЙ В SUPPLIERS ===");
    using (var cmd = new SqlCommand(@"
        SELECT TOP 5 SupplierCode, CompanyName, BusinessId 
        FROM Suppliers 
        WHERE SupplierCode >= 300000
        ORDER BY Id DESC", conn)) {
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"Code: {reader["SupplierCode"]}, Name: {reader["CompanyName"]}, BusinessId: {reader["BusinessId"]}");
            }
        }
    }
}
