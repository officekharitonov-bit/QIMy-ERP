#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.11"

using Microsoft.Data.Sqlite;

var connectionString = "Data Source=C:\\Projects\\QIMy\\src\\QIMy.Web\\QImyDb.db";
using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    
    var sql = "SELECT Id, UserName, Email, NormalizedUserName, NormalizedEmail, PasswordHash FROM AspNetUsers";
    
    using (var command = new SqliteCommand(sql, connection))
    using (var reader = command.ExecuteReader())
    {
        Console.WriteLine("Users in database:");
        Console.WriteLine("==================");
        while (reader.Read())
        {
            Console.WriteLine($"Id: {reader["Id"]}");
            Console.WriteLine($"UserName: {reader["UserName"]}");
            Console.WriteLine($"Email: {reader["Email"]}");
            Console.WriteLine($"NormalizedUserName: {reader["NormalizedUserName"]}");
            Console.WriteLine($"NormalizedEmail: {reader["NormalizedEmail"]}");
            Console.WriteLine($"PasswordHash: {reader["PasswordHash"]}");
            Console.WriteLine("------------------");
        }
    }
}
