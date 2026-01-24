namespace QIMy.Core.Entities;

/// <summary>
/// Связь между пользователем и его бизнесами (многие ко многим)
/// Пользователь может управлять несколькими компаниями
/// </summary>
public class UserBusiness : BaseEntity
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ID бизнеса
    /// </summary>
    public int BusinessId { get; set; }

    /// <summary>
    /// Роль пользователя в этом бизнесе (Owner, Manager, Employee)
    /// </summary>
    public string Role { get; set; } = "Employee";

    /// <summary>
    /// Бизнес активный по умолчанию при входе
    /// </summary>
    public bool IsDefault { get; set; }

    // Navigation properties
    public AppUser User { get; set; } = null!;
    public Business Business { get; set; } = null!;
}
