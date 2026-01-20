#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.AspNetCore.Identity, 2.2.0"
#r "nuget: Microsoft.Data.Sqlite, 8.0.11"

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using System;

// Хешируем пароль
var hasher = new PasswordHasher<object>();
var passwordHash = hasher.HashPassword(null, "Admin123!");

Console.WriteLine($"Password Hash: {passwordHash}");

// Подключаемся к БД
var connectionString = "Data Source=C:\\Projects\\QIMy\\src\\QIMy.Web\\QImyDb.db";
using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    
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
    
    using (var command = new SqliteCommand(sql, connection))
    {
        command.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
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
        command.Parameters.AddWithValue("@FirstName", "Egor");
        command.Parameters.AddWithValue("@LastName", "Kharitonov");
        command.Parameters.AddWithValue("@BusinessId", DBNull.Value);
        command.Parameters.AddWithValue("@Role", "Admin");
        command.Parameters.AddWithValue("@IsActive", 1);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@UpdatedAt", DBNull.Value);
        
        var result = command.ExecuteNonQuery();
        Console.WriteLine($"✅ User created successfully! Rows affected: {result}");
        Console.WriteLine("Email: office@kharitonov.at");
        Console.WriteLine("Password: Admin123!");
    }
}
