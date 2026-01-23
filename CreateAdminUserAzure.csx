#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.AspNetCore.Identity, 2.2.0"
#r "nuget: Microsoft.Data.SqlClient, 5.1.5"

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System;

// Хешируем пароль
var hasher = new PasswordHasher<object>();
var passwordHash = hasher.HashPassword(null, "Admin123!");

Console.WriteLine($"Password Hash: {passwordHash}");

// Подключаемся к Azure SQL
var connectionString = "Server=tcp:qimy-sql-server.database.windows.net,1433;Initial Catalog=QImyDb;Persist Security Info=False;User ID=qimyadmin;Password=h970334054CRgd1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    Console.WriteLine("Connected to Azure SQL!");

    var sql = @"
        INSERT INTO AspNetUsers (
            Id, UserName, NormalizedUserName, Email, NormalizedEmail,
            EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd,
            LockoutEnabled, AccessFailedCount, FirstName, LastName, BusinessId,
            Role, IsActive, CreatedAt, UpdatedAt
        ) VALUES (
            @Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail,
            @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp,
            @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnd,
            @LockoutEnabled, @AccessFailedCount, @FirstName, @LastName, @BusinessId,
            @Role, @IsActive, @CreatedAt, @UpdatedAt
        )";

    using (var command = new SqlCommand(sql, connection))
    {
        var userId = Guid.NewGuid().ToString();
        command.Parameters.AddWithValue("@Id", userId);
        command.Parameters.AddWithValue("@UserName", "office@kharitonov.at");
        command.Parameters.AddWithValue("@NormalizedUserName", "OFFICE@KHARITONOV.AT");
        command.Parameters.AddWithValue("@Email", "office@kharitonov.at");
        command.Parameters.AddWithValue("@NormalizedEmail", "OFFICE@KHARITONOV.AT");
        command.Parameters.AddWithValue("@EmailConfirmed", 1);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@SecurityStamp", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("@ConcurrencyStamp", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("@PhoneNumber", DBNull.Value);
        command.Parameters.AddWithValue("@PhoneNumberConfirmed", 0);
        command.Parameters.AddWithValue("@TwoFactorEnabled", 0);
        command.Parameters.AddWithValue("@LockoutEnd", DBNull.Value);
        command.Parameters.AddWithValue("@LockoutEnabled", 1);
        command.Parameters.AddWithValue("@AccessFailedCount", 0);
        command.Parameters.AddWithValue("@FirstName", "Admin");
        command.Parameters.AddWithValue("@LastName", "User");
        command.Parameters.AddWithValue("@BusinessId", DBNull.Value);
        command.Parameters.AddWithValue("@Role", "Admin");
        command.Parameters.AddWithValue("@IsActive", 1);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var rowsAffected = command.ExecuteNonQuery();
        Console.WriteLine($"User created successfully! ID: {userId}");
        Console.WriteLine($"Email: office@kharitonov.at");
        Console.WriteLine($"Password: Admin123!");
    }
}
