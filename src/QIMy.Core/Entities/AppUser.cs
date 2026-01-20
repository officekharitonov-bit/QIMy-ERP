using Microsoft.AspNetCore.Identity;

namespace QIMy.Core.Entities;

/// <summary>
/// Пользователь системы с поддержкой мультитенантности
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// ID бизнеса (организации) - для мультитенантности
    /// </summary>
    public int? BusinessId { get; set; }

    /// <summary>
    /// Связь с организацией
    /// </summary>
    public Business? Business { get; set; }

    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public string Role { get; set; } = "User"; // Admin, User, Accountant

    /// <summary>
    /// Активен ли пользователь
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Полное имя пользователя
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
