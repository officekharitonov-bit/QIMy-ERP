using System;
using System.Data.SqlClient;

var connString = @"Server=tcp:qimy-server.database.windows.net,1433;Initial Catalog=qimy-db;Persist Security Info=False;User ID=qimyadmin;Password=QIMy2024!Secure;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

using (var conn = new SqlConnection(connString)) {
    conn.Open();
    
    Console.WriteLine("=== Удаляю клиентов с кодами поставщиков (300000-399999) ===");
    
    using (var cmd = new SqlCommand(@"
        DELETE FROM Clients 
        WHERE ClientCode >= 300000 AND ClientCode <= 399999", conn)) {
        var deleted = cmd.ExecuteNonQuery();
        Console.WriteLine($"✅ Удалено {deleted} записей");
    }
    
    Console.WriteLine("\n=== Текущее состояние ===");
    using (var cmd = new SqlCommand(@"
        SELECT COUNT(*) as Total, BusinessId 
        FROM Clients 
        GROUP BY BusinessId
        ORDER BY BusinessId", conn)) {
        using (var reader = cmd.ExecuteReader()) {
            Console.WriteLine("Clients:");
            while (reader.Read()) {
                Console.WriteLine($"  BusinessId={reader["BusinessId"]}: {reader["Total"]} клиентов");
            }
        }
    }
}
